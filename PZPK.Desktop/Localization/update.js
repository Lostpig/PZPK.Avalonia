const fs = require('fs')
const path = require('path')

const loadLanguageJson = () => {
	const langFile = path.join(__dirname, "languages.json")
	const text = fs.readFileSync(langFile, { encoding: "utf8" })
	const langData = JSON.parse(text)

	return langData
}
const getLanguageItemJsonFileData = (file) => {
	let langItemJson = {
		namespaces: []
	}
	if (fs.existsSync(file)) {
		const itemText = fs.readFileSync(file, { encoding: "utf8" })
		const itemJson = JSON.parse(itemText)

		if (!itemJson.namespaces) {
			console.log(`error: ${file} is not a language json file!`)
		} else {
			langItemJson = itemJson
		}
	}

	return langItemJson
}
const createLanguageItemJson = (langItem, namespaces) => {
	const itemFile = path.join(__dirname, langItem.value + ".json")
	var data = getLanguageItemJsonFileData(itemFile)

	for(let ns of namespaces) {
		let nsItem = data.namespaces.find((n) => n.namespace == ns.namespace)
		if (!nsItem) {
			nsItem = { namespace: ns.namespace, fields: {} }
			data.namespaces.push(nsItem)
		}

		for(let f of ns.fields) {
			if (!nsItem.fields[f]) {
				nsItem.fields[f] = 'MISS_' + f
			}
		}
	}

	const dataText = JSON.stringify(data, null, 2)
	fs.writeFileSync(itemFile, dataText, { encoding: "utf8" })

	console.log(`${langItem.name} json file upadated.`)
}

// excute
const languageJson = loadLanguageJson()
for (let lang of languageJson.languages) {
	createLanguageItemJson(lang, languageJson.namespaces)
}
console.log(`update completed.`)