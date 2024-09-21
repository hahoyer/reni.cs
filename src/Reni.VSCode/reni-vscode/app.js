const path = require("path");

const baseNetAppPath = path.join(__dirname, "bin\\Debug\\net8.0");

process.env.EDGE_USE_CORECLR = 1;
process.env.EDGE_APP_ROOT = baseNetAppPath;

var edge = require("electron-edge-js");

const reni2dll = path.join(baseNetAppPath, "ReniLSP.dll");
var reni2 = edge.func(reni2dll);

reni2('', function(error, result) {
  if (error) throw error;
  window.webContents.send("fromMain", 'getItem', JSON.stringify( result, null, 2 ));
});
