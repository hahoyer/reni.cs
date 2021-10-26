using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace LanguageServer
{
    [ContentType("reni")]
    [Export(typeof(ILanguageClient))]
    public class ReniLanguageClient : ILanguageClient
    {
        const string LocationRoot = "c:\\data\\develop\\git\\reni.cs";
        public IEnumerable<string> ConfigurationSections => null;

        public IEnumerable<string> FilesToWatch => null;

        public object InitializationOptions => null;
        public string Name => "Reni Language Extension";

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            var serverPath = Path.Combine(LocationRoot, "out/Debug/ReniLSP.exe");
            var info = new ProcessStartInfo
            {
                FileName = serverPath
                , RedirectStandardInput = true
                , RedirectStandardOutput = true
                , UseShellExecute = false
                , CreateNoWindow = true
            };

            var process = new Process();
            process.StartInfo = info;

            return process.Start()
                ? new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream)
                : null;
        }

        public async Task OnLoadedAsync() => await StartAsync.InvokeAsync(this, EventArgs.Empty);

        public Task OnServerInitializedAsync() => Task.CompletedTask;

        public Task OnServerInitializeFailedAsync(Exception e) => Task.CompletedTask;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;
    }
}