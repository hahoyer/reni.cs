const path = require("path");

const dllPath = "a:/develop/Reni/dev/out/Debug/net8.0";
const name = "ReniLSP";
const fullName = path.join(dllPath, name + ".dll");

// process.env.EDGE_USE_CORECLR = 1;
// process.env.EDGE_APP_ROOT = dllPath;

var edge = require("electron-edge-js");

function activate(context) {
  const server = edge.func(fullName);
  const fff = 3
}

// This method is called when your extension is deactivated
function deactivate() {}

module.exports = {
  activate,
  deactivate,
};
