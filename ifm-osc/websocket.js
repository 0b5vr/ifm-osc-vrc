const { WebSocketServer } = require( 'ws' );
const { IFMHandler } = require( './IFMHandler' );

const wsServer = new WebSocketServer( {
  port: 8008,
} );

const ifmHandler = new IFMHandler();

wsServer.on( 'connection', ( ws ) => {
  console.info( 'someone connected to our websocket server' );

  const handler = ( event ) => {
    ws.send( JSON.stringify( event ) );
  };

  ifmHandler.on( 'message', handler );

  ws.on( 'close', () => {
    console.info( 'someone disconnected from our websocket server' );

    ifmHandler.off( 'message', handler );
  } );
} );
