const dgram = require( 'dgram' );
const EventEmitter = require( 'eventemitter3' );
const blendshapeNameProperNameMap = require('./blendshapeNameProperNameMap');

class IFMHandler extends EventEmitter {
  constructor() {
    super();

    this.socket = dgram.createSocket( 'udp4' );

    this.socket.on( 'listening', () => {
      const address = this.socket.address();
      console.log( 'UDP socket listening on ' + address.address + ":" + address.port );
    } );

    this.socket.on( 'message', ( message, remote ) => {
      const msg = message.toString();

      const equal = msg.split( '=' );
      const pipe = equal.map( ( msg ) => msg.split( '|' ) );

      const result = {};

      // blendshapes
      result.blendshapes = {};

      pipe[ 0 ].map( ( entry ) => {
        const hyphen = entry.split( '-' );

        if ( hyphen.length !== 1 ) {
          const name = hyphen[ 0 ];
          const properName = blendshapeNameProperNameMap[ name ];
          const value = parseInt( hyphen[ 1 ], 10 );

          result.blendshapes[ properName ] = value;
        }
      } );

      // head, eyes
      pipe[ 1 ].map( ( entry ) => {
        const sharp = entry.split( '#' );

        if ( sharp.length !== 1 ) {
          const name = sharp[ 0 ];
          const values = sharp[ 1 ].split( ',' ).map( ( v ) => parseFloat( v ) );

          result[ name ] = values;
        }
      } );

      // emit
      this.emit( 'message', result );
    } );

    this.socket.bind( 49983 );

    {
      const magicData = 'iFacialMocap_sahuasouryya9218sauhuiayeta91555dy3719';
      this.socket.send( magicData, 49983, '192.168.0.79' );
    }
  }
}

module.exports = { IFMHandler };
