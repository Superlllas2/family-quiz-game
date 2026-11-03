// Lets `node --test tools/` run every tools/*.test.mjs on every Node version. Node 20 searches a directory for test
// files itself (and never runs this file: index.js is not a test file name); Node 22 and later take the argument as a
// file pattern, load the directory as a module, and so reach this file, which loads the tests.
const { readdirSync } = require("node:fs");
const { join } = require("node:path");
const { pathToFileURL } = require("node:url");

for (const file of readdirSync(__dirname).filter((f) => f.endsWith(".test.mjs")).sort()) {
  import(pathToFileURL(join(__dirname, file)).href);
}
