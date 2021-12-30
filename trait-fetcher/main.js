const request = require('axios');
const fs = require('fs');

async function fetch(id) {
	let results = await request.get("https://storage.googleapis.com/axolittles-cdn/metadata/" + id)
	try {
		let json = results.data
		return json;
	} catch(err) {
		console.log(err)
		return {}
	}
}

const intervalMax = 400;

(async function () {
	let sealDict = {}
	
	let fetched = 0;
	for (let i = 0; i < 10000; i++) {
		fetch(i).then(axo => {
			sealDict[fetched] = { 
				"id": axo.title.substring(axo.title.indexOf('#') + 1),
				"background": axo.attributes[0].value,
				"top":axo.attributes[1].value,
				"face":axo.attributes[2].value,
				"outfit":axo.attributes[3].value,
				"type":axo.attributes[4].value
			};
			console.log(sealDict[fetched])
			console.log(fetched);
			fetched++;
		});
		
		if (i != 0 && i % intervalMax == 0) {
			const taskWait = new Promise((resolve, reject) => {
				let timer = setInterval(() => {
					if (fetched >= i) {
						clearInterval(timer);
						resolve();
					}
				}, 1000);
			});
			
			console.log("Waiting for " + intervalMax + " to finish.");
			await taskWait;
		}
	}
		
	const myPromise = new Promise((resolve, reject) => {
		let timer = setInterval(() => {
			if (fetched >= 9999) {
				clearInterval(timer);
				resolve();
			}
		}, 1000);
	});
			
	await myPromise;
			
	let orderedSet = []
	for (let i = 0; i < 10000; i++) {
		orderedSet.push(sealDict[i]);
	}
	
	console.log("IMPORTING EXISTING TRAITS");
	// IMPORT EXISTING TRAITS
	var obj = JSON.parse(fs.readFileSync('raw_attributes.json', 'utf8'));
	for (let i = 0; i < 10000; i++) {
		if (orderedSet[i] != undefined) {
			orderedSet[i]["rhue"] = obj[i].hue;
			orderedSet[i]["rtop"] = obj[i].top;
			orderedSet[i]["rface"] = obj[i].face;
			orderedSet[i]["routfit"] = obj[i].outfit;
			orderedSet[i]["rbodytype"] = obj[i].bodytype;
			console.log(i);
		}
	}
	console.log("DONE..");
	
	fs.writeFile("axolittles.json", JSON.stringify(orderedSet), function(err, result) {
		if(err) console.log('error', err);
	});
	
})();