#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace ReniLanguageServer
{
    [ContentType("reni")]
    [Export(typeof(ILanguageClient))]
    public class ReniLanguageClient : DumpableObject, ILanguageClient
    {
        static readonly string[] LocationRoot =
        {
            "c:/data/develop/git/reni.cs", "a:/develop/Reni/dev",
        };

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            Debugger.Launch();
            await Task.Yield();

            //var process = Process.GetProcessesByName("ReniLSP").SingleOrDefault();
            var process = CreateProcess();
            if(process == null)
                return null;

            var result = new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            return result;
        }


        public IEnumerable<string> ConfigurationSections => null;

        public IEnumerable<string> FilesToWatch => null;

        public object InitializationOptions => null;
        public string Name => "Reni Language Extension";


        public async Task OnLoadedAsync() => await StartAsync.InvokeAsync(this, EventArgs.Empty);

        public Task OnServerInitializedAsync() => Task.CompletedTask;

        Task<InitializationFailureContext?> ILanguageClient.OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
        {
            NotImplementedMethod(initializationState);
            return Task.FromResult<InitializationFailureContext?>(null);
        }

        bool ILanguageClient.ShowNotificationOnInitializeFailed
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public Task OnServerInitializeFailedAsync(Exception e) => Task.CompletedTask;

        static Process CreateProcess()
        {
            var serverPath = GetServerPath();
            var info = new ProcessStartInfo
            {
                FileName = serverPath
                , RedirectStandardInput = true
                , RedirectStandardOutput = true
                , UseShellExecute = false
                , CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = info
            };

            return process.Start()? process : null;
        }

        static string GetServerPath()
        {
            var enumerable = LocationRoot
                .Select(location => location.PathCombine("out/Debug/ReniLSP.exe").ToSmbFile())
                .ToArray();
            return enumerable
                .Single(location => location.Exists)
                .FullName;
        }
    }
}