var edge = require('electron-edge-js');
const path = require("path");
const baseNetAppPath = path.join(__dirname, "bin\\Debug\\net8.0");
const reni2dll = path.join(baseNetAppPath, "ReniLSP.dll");
var reni2 = edge.func(reni2dll);

exports.run = function (window) {

    reni2('', function(error, result) {
        if (error) throw error;
        window.webContents.send("fromMain", 'getItem', JSON.stringify( result, null, 2 ));
    });

    return;
}