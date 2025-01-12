"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const net_1 = require("net");
const vscode_1 = require("vscode");
const node_1 = require("vscode-languageclient/node");
let client;
function activate() {
    const connectFunc = () => {
        return new Promise((resolve) => {
            function tryConnect() {
                const socket = (0, net_1.connect)(`\\\\.\\pipe\\Reni.LanguageServer`);
                socket.on("connect", () => {
                    resolve({ writer: socket, reader: socket });
                });
                socket.on("error", (e) => {
                    setTimeout(tryConnect, 5000);
                });
            }
            tryConnect();
        });
    };
    client = new node_1.LanguageClient("reni", connectFunc, {
        documentSelector: [
            {
                language: "reni",
            },
            {
                pattern: "**/*.reni",
            },
        ],
        progressOnInitialization: true,
        connectionOptions: {
            maxRestartCount: 10,
            cancellationStrategy: node_1.CancellationStrategy.Message,
        },
        middleware: {
            executeCommand: async (command, args, next) => {
                const choices = [];
                const quickPick = vscode_1.window.createQuickPick();
                quickPick.title = 'Enter methodcall :';
                quickPick.items = choices.map(choice => ({ label: choice }));
                quickPick.onDidChangeValue(() => {
                    if (!choices.includes(quickPick.value)) {
                        quickPick.items = [quickPick.value, ...choices].map(label => ({ label }));
                    }
                });
                quickPick.onDidAccept(() => {
                    const selection = quickPick.activeItems[0];
                    args = args.slice(0);
                    args.push(selection);
                    args.push(vscode_1.window.activeTextEditor?.document.uri.fsPath);
                    quickPick.hide();
                    return next(command, args);
                });
                quickPick.show();
            }
        }
    });
    let red = vscode_1.Uri.parse('https://imgur.com/FQM0ACT.png'); //use actual icons
    let green = vscode_1.Uri.parse('https://imgur.com/1cH046F.png'); //use actual icons
    let decorationTypes = new Map();
    client.onNotification('testRunnerNotification', (testMessage) => {
        var image = testMessage.state === 0 ? red : green;
        var position = new vscode_1.Range(testMessage.lineNumber, 0, testMessage.lineNumber, 0);
        let decorationType = vscode_1.window.createTextEditorDecorationType({
            gutterIconPath: image,
            gutterIconSize: 'contain'
        });
        const editor = vscode_1.window.activeTextEditor;
        if (editor !== undefined) {
            var possibleDecoration = decorationTypes.get(testMessage.lineNumber);
            if (possibleDecoration === undefined) {
                decorationTypes.set(testMessage.lineNumber, decorationType);
                editor.setDecorations(decorationType, [{
                        range: position
                    }]);
            }
            else {
                editor.setDecorations(possibleDecoration, []);
                decorationTypes.delete(testMessage.lineNumber);
                decorationTypes.set(testMessage.lineNumber, decorationType);
                editor.setDecorations(decorationType, [{ range: position }]);
            }
        }
    });
    client.onNotification('valueEvaluationNotification', (variableStateNotificationMessage) => {
        const editor = vscode_1.window.activeTextEditor;
        for (let key in variableStateNotificationMessage.lineTextPair) {
            let variableValue = variableStateNotificationMessage.lineTextPair[key];
            const decorationType = vscode_1.window.createTextEditorDecorationType({
                after: {
                    contentText: '  ' + variableValue,
                    color: '#a9a9a9'
                }
            });
            if (editor) {
                const range = new vscode_1.Range(Number(key), editor.document.lineAt(Number(key)).text.length, Number(key), editor.document.lineAt(Number(key)).text.length);
                const decoration = {
                    range: range,
                    renderOptions: {}
                };
                editor.setDecorations(decorationType, []);
                editor.setDecorations(decorationType, [decoration]);
            }
        }
    });
    client.registerProposedFeatures();
    client.start();
}
function deactivate() {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
//# sourceMappingURL=extension.js.map