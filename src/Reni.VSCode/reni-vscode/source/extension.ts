import { connect } from "net";
import path = require("path");
import { Range, Uri, window as Window, TextEditorDecorationType } from 'vscode';
import {
  LanguageClient,
  StreamInfo,
  CancellationStrategy,
  integer,
} from "vscode-languageclient/node";

let client: LanguageClient;
export function activate() {
  const connectFunc = () => {
    return new Promise<StreamInfo>((resolve) => {
      function tryConnect() {
        const socket = connect(`\\\\.\\pipe\\Reni.LanguageServer`);
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

  client = new LanguageClient("reni", connectFunc, {
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
      cancellationStrategy: CancellationStrategy.Message,
    },
    middleware: {
      executeCommand: async (command, args, next) => {
        const choices: string[] = [];
        const quickPick = Window.createQuickPick();
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
          args.push(Window.activeTextEditor?.document.uri.fsPath);
          quickPick.hide();
          return next(command, args);
        });
        quickPick.show();
      }
    }
  });
  let red = Uri.parse('https://imgur.com/FQM0ACT.png'); //use actual icons
  let green = Uri.parse('https://imgur.com/1cH046F.png'); //use actual icons
  let decorationTypes = new Map<integer, TextEditorDecorationType>();
  client.onNotification('testRunnerNotification', (testMessage: any) => {
    var image = testMessage.state === 0 ? red : green;
    var position = new Range(testMessage.lineNumber, 0, testMessage.lineNumber, 0);
    let decorationType = Window.createTextEditorDecorationType({
      gutterIconPath: image,
      gutterIconSize: 'contain'
    });
    const editor = Window.activeTextEditor;
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
  client.onNotification('valueEvaluationNotification', (variableStateNotificationMessage: any) =>{
    const editor = Window.activeTextEditor;

    for(let key in variableStateNotificationMessage.lineTextPair){
      let variableValue = variableStateNotificationMessage.lineTextPair[key];
    const decorationType = Window.createTextEditorDecorationType({
      after: {
          contentText: '  ' + variableValue,
          color: '#a9a9a9'
      }
  });
    if (editor) {
      const range = new Range(Number(key), editor.document.lineAt(Number(key)).text.length, Number(key), editor.document.lineAt(Number(key)).text.length);

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

export function deactivate(): Thenable<void> | undefined {
  if (!client) {
    return undefined;
  }
  return client.stop();
}