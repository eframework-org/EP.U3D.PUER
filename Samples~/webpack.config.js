const fs = require("fs")
const path = require("path")
const package = require(path.resolve(__dirname, "package.json"))

const entries = {}
Object.keys(package.dependencies).forEach(dep => {
    const dpackage = require(path.resolve(__dirname, `node_modules/${dep}/package.json`))
    if (dpackage.module || dpackage.browser || dpackage.main) {
        let name = ""
        if (dpackage.module) name = dpackage.module
        else if (dpackage.browser) name = dpackage.browser
        else if (dpackage.main) name = dpackage.main
        const entry = path.resolve(`./node_modules/${dep}/${name}`)
        if (fs.existsSync(entry)) {
            entries[dep] = entry
            console.info(`module entry: ${entry}`)
        } else {
            console.error(`entry not found: ${entry}`)
        }
    } else {
        console.error(`module not defined: ${dep}`)
    }
})

module.exports = {
    entry: entries,
    output: {
        filename: "[name].js",
        path: path.resolve(__dirname, "Assets/Temp/TypeScripts"),
        library: { type: "module" },
    },
    mode: "production",
    target: "web",
    experiments: { outputModule: true }
}