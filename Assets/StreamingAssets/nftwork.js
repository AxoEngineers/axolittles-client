// Require.js allows us to configure shortcut alias
require.config({
	
  // The shim config allows us to configure dependencies for
  // scripts that do not call define() to register a module
  shim: {
    'socketio': {
      exports: 'io'
    }
  },
  
  paths: {
    socketio: 'socket.io'
  }
  
});

define(['socket.io'], function( io ) {

	return io;

});