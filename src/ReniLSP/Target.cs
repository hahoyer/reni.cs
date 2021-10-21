using System.Collections.Generic;
using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace ReniLSP
{
    class Target
    {
        readonly IDictionary<string, JToken> Documents = new Dictionary<string, JToken>();

        [JsonRpcMethod("initialize")]
        public object Initialize(JToken arg)
        {
            Debugger.Launch();

            var result = new InitializeResult
            {
                Capabilities = capabilities
            };

            return result;
        }

        [JsonRpcMethod("initiaialized")]
        public void Initialized(JToken arg)
        {
            var a = arg.ToObject<InitializedParams>();
            new Registration();
            (Methods.InitializedName + " " + Tracer.Dump(a)).Log();
        }

        [JsonRpcMethod("textDocument/didOpen")]
        public void TextDocumentDidOpen(JToken arg)
        {
            var item = arg.ToObject<DidOpenTextDocumentParams>().AssertNotNull().TextDocument;
            Documents[item.Uri.AbsolutePath] = item;
        }
    }
}