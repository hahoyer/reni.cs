var languageclient = require("vscode-languageclient");
process.env.EDGE_USE_CORECLR = 1;
var edge = require("electron-edge-js");
const path = require("path");
//const baseNetAppPath = path.join(__dirname, "bin/Debug/net8.0");
const baseNetAppPath = path.join(
  __dirname,
  "../../../out/Debug/net8.0-windows"
);
var ServerPipeTask;

function activate(context) {
  const reni2dll = path.join(baseNetAppPath, "ReniLSP.dll");
  var reni2 = edge.func({
    assemblyFile: reni2dll,
    typeName: "ReniLSP.MainContainer",
    methodName: "RunServer",
  });

  var pair = FullDuplexStream.CreatePair();
  var reader = pipeToLSP.UsePipeReader();
  var writer = pipeToLSP.UsePipeWriter();

  reni2(reader, writer, function (error, result) {
    if (error) throw error;

  });

  ServerPipeTask = Task.Run(() => RunServer(reader, writer), token);
  var duplexPipe = new DuplexPipe(
    pipeToVS.UsePipeReader(),
    pipeToVS.UsePipeWriter()
  );

  return Task.FromResult < IDuplexPipe > duplexPipe;
}

// This method is called when your extension is deactivated
function deactivate() {}

module.exports = {
  activate,
  deactivate,
};
