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

	const oldData = getLanguageItemJsonFileData(itemFile)
	const newData = { namespaces: [] }

	for (let ns of namespaces) {
		const oldNs = oldData.namespaces.find((n) => n.namespace == ns.namespace)
		const newNs = { namespace: ns.namespace, fields: {} }

		for(let f of ns.fields) {
			if (oldNs && oldNs.fields[f]) {
				newNs.fields[f] = oldNs.fields[f]
			} else {
				newNs.fields[f] = 'MISS_' + f
			}
		}

		newData.namespaces.push(newNs)
	}

	const dataText = JSON.stringify(newData, null, 2)
	fs.writeFileSync(itemFile, dataText, { encoding: "utf8" })

	console.log(`${langItem.name} json file upadated.`)
}

// excute
const languageJson = loadLanguageJson()
for (let lang of languageJson.languages) {
	createLanguageItemJson(lang, languageJson.namespaces)
}
console.log(`update completed.`)