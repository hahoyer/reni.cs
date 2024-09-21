process.env['ELECTRON_DISABLE_SECURITY_WARNINGS']=true
const path = require("path");

const baseNetAppPath = path.join(__dirname, "bin\\Debug\\net8.0");

process.env.EDGE_USE_CORECLR = 1;
process.env.EDGE_APP_ROOT = baseNetAppPath;
var edge = require("electron-edge-js");


function activate(context) {
  const reni2dll = path.join(baseNetAppPath, "ReniLSP.dll");
  var reni2 = edge.func(reni2dll);
  reni2('', function(error, result) {
    if (error) throw error;
    window.webContents.send("fromMain", 'getItem', JSON.stringify( result, null, 2 ));
  });

  const fff = 3;
}

// This method is called when your extension is deactivated
function deactivate() {}

module.exports = {
  activate,
  deactivate,
};
