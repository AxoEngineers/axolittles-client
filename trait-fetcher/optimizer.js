const fs = require('fs');

async function fetchSeal(id) {
	let results = await request.get("https://cloudflare-ipfs.com/ipfs/QmXUUXRSAJeb4u8p4yKHmXN1iAKtAV7jwLHjw35TNm5jN7/" + id)
	try {
		let json = results.data
		return json;
	} catch(err) {
		console.log(err)
		return {}
	}
}

(async function () {
	
	let traitIndices = []
	let traitKeyMap = {}
	
	let indices = []
	let keyMap = {}
	
	let newSealMap = []
	let indexCounter = 0
	let traitIndexCounter = 0
	
	var config = require('./seals.json');
	for (const [id, seal] of Object.entries(config)) {
		let attributeSet = ""
		
		seal.attributes.forEach(attribute =>
		{
			let traitType = attribute.trait_type
			if (!traitKeyMap[traitType])
			{
				traitIndices.push(traitType)
				traitIndexCounter++;
				traitKeyMap[traitType] = traitIndexCounter
			}
			
			let key = traitKeyMap[traitType]+"|"+attribute.value;
			if (!keyMap[key])
			{
				indices.push(key)
				indexCounter++
				keyMap[key] = indexCounter
			}
			
			// build new format
			if (attributeSet.length == 0)
				attributeSet = "" + keyMap[key] + "|"
			else
				attributeSet += keyMap[key] + "|"
		});
		
		if (attributeSet.length > 0)
			attributeSet = attributeSet.substring(0, attributeSet.length - 1)
		
		newSealMap.push(attributeSet)
	}
	
	const finalObject = {
		traits: traitIndices,
		keys: indices,
		values: newSealMap
	}
	
	fs.writeFile("seals-optimized-final.json", JSON.stringify(finalObject), function(err, result) {
		if(err) console.log('error', err);
	});
})();