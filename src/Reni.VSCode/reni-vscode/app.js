var edge = require("edge-js");

const vscode = require("vscode");
//const languageClient = require("vscode-languageclient");

function activate(context) {
  //const server = 'edge.func("A:\\develop\\Reni\\dev\\out\\Debug\\net472\\ReniLSP.dll")';

  const disposable = vscode.commands.registerCommand(
    "reni-vscode.start-lsp",
    function () {
      vscode.window.showInformationMessage("Hello World from reni-vscode!");
    }
  );

  context.subscriptions.push(disposable);
}

// This method is called when your extension is deactivated
function deactivate() {}

module.exports = {
  activate,
  deactivate,
};
