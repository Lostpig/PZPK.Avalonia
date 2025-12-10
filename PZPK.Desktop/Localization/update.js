const fs = require('fs')
const path = require('path')

const langFile = path.join(__dirname, "languages.json")
const text = fs.readFileSync(langFile, { encoding: "utf8" })

const langData = JSON.parse(text)

for(let item of langData.languages) {
	const itemFile = path.join(__dirname, item.value + ".json")

	let fields = {}
	if (fs.existsSync(itemFile)) {
		const itemText = fs.readFileSync(itemFile, { encoding: "utf8" })
		const itemJson = JSON.parse(itemText)

		fields = itemJson
	}

	for(let f of langData.fields) {
		if (!fields[f]) {
			fields[f] = 'MISS_' + f
		}
	}

	const fieldsText = JSON.stringify(fields, null, 2)
	fs.writeFileSync(itemFile, fieldsText, { encoding: "utf8" })

	console.log(`${item.name} json file upadated.`)
}

console.log(`complete.`)