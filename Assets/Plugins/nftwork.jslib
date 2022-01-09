
mergeInto(LibraryManager.library, {

	MetamaskAuthenticate: function (web3URL) {
		var apiURL = UTF8ToString(web3URL)
		
		require(['nftauth'], function (auth) {
			auth.ConnectWallet(apiURL,
				function(address, signature, nfts) {
					window.unityInstance.SendMessage("MetamaskAuth", "CompleteMetamaskAuth", address + "|" + signature + "|" + nfts);
				}, 
				function(code, message) { 
					console.log("Error (" + code + "), " + JSON.stringify(message));
					window.unityInstance.SendMessage("MetamaskAuth", "CompleteMetamaskError", code + "|" + message);
				}
			);
		});
	},
	
	GetURL: function () {
		var url = window.location.href;
		var bufferSize = lengthBytesUTF8(url) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(url, buffer, bufferSize);
		return buffer;
	},
		
	GetPlayerTime: function() {
		var date = new Date().toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true });
		var bufferSize = lengthBytesUTF8(date) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(date, buffer, bufferSize);
		return buffer;
	}

});