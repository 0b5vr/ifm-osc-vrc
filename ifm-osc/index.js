const dgram = require( 'dgram' );
const osc = require( 'osc-min' );
const blendshapeTypeMap = require('./blendshapeTypeMap');
const { IFMHandler } = require( './IFMHandler' );
const range = require( './range' );
const clamp = require( './clamp' );

const VRC_IN_PORT = 9000;
const VRC_OUT_PORT = 9001;

const socketVRC = dgram.createSocket( 'udp4' );

const ifmHandler = new IFMHandler();

function send( address, args ) {
  const msg = osc.toBuffer( {
    address,
    args,
  } );
  socketVRC.send( msg, 0, msg.length, VRC_IN_PORT );
}

function rangeRot( x, l, h ) {
  return clamp(
    range( x, l, h, -1.0, 1.0 ),
    -1.0,
    1.0,
  );
}

ifmHandler.on( 'message', ( { blendshapes, head, leftEye, rightEye } ) => {
  for ( const [ name, value ] of Object.entries( blendshapes ) ) {
    const psType = blendshapeTypeMap[ name ];

    if ( psType === 'none' ) {
      continue;
    } else if ( psType === 'bool1' ) {
      const bValue = value >= 80;
      const iValue = bValue ? 1 : 0;
      send( `/iFacialMocap/blendshapes/${ name }/0`, [ iValue ] );
    } else if ( psType === 'bool2' ) {
      for ( let i = 0; i < 2; i ++ ) {
        const bValue = ( Math.floor( value / 30.0 ) & Math.pow( 2, i ) ) > 0;
        const iValue = bValue ? 1 : 0;
        send( `/iFacialMocap/blendshapes/${ name }/${ i }`, [ iValue ] );
      }
    } else if ( psType === 'float8' ) {
      send( `/iFacialMocap/blendshapes/${ name }`, [ value / 100.0 ] );
    }
  }

  send( '/iFacialMocap/head/x', rangeRot( head[ 0 ], -40.0, 40.0 ) );
  send( '/iFacialMocap/head/y', rangeRot( -head[ 1 ], -40.0, 40.0 ) );
  send( '/iFacialMocap/head/z', rangeRot( -head[ 2 ], -40.0, 40.0 ) );

  const eyeX = 0.5 * ( leftEye[ 0 ] + rightEye[ 0 ] );
  const eyeY = 0.5 * ( leftEye[ 1 ] + rightEye[ 1 ] );
  send( '/iFacialMocap/eyes/x', rangeRot( eyeX, -30.0, 30.0 ) );
  send( '/iFacialMocap/eyes/y', rangeRot( -eyeY, -30.0, 30.0 ) );
} );

socketVRC.bind( VRC_OUT_PORT );
