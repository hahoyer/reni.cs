using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using ReniLSP;

namespace ReniVSIX;

[ContentType(Constants.LanguageName)]
[Export(typeof(ILanguageClient))]
[RunOnContext(RunningContext.RunOnHost)]
[ProvideLanguageEditorOptionPage(typeof(ConfigurationProperties), Constants.LanguageName, "Formatting", "", "100")]
public class LanguageClient : ILanguageClient
{
    [UsedImplicitly]
    Task Server;

    async Task<Connection> ILanguageClient.ActivateAsync(CancellationToken token)
    {
        await Task.Yield();

        var reader = new Pipe();
        var writer = new Pipe();

        Server = Task.Run(() => MainContainer.RunServer(writer.Reader, reader.Writer), token);
        return new(reader.Reader.AsStream(), writer.Writer.AsStream());
    }

    IEnumerable<string> ILanguageClient.ConfigurationSections
    {
        get
        {
            yield return "formatting";
            yield return "Formatting";
        }
    }


    IEnumerable<string> ILanguageClient.FilesToWatch => null;

    object ILanguageClient.InitializationOptions => null;
    string ILanguageClient.Name => "Reni Language Extension";

    async Task ILanguageClient.OnLoadedAsync() => await StartAsync.InvokeAsync(this, EventArgs.Empty);

    Task ILanguageClient.OnServerInitializedAsync() => Task.CompletedTask;

    Task<InitializationFailureContext> ILanguageClient.OnServerInitializeFailedAsync
        (ILanguageClientInitializationInfo initializationState)
        => Task.FromResult<InitializationFailureContext>(new());

    bool ILanguageClient.ShowNotificationOnInitializeFailed => true;

    event AsyncEventHandler<EventArgs> ILanguageClient.StartAsync
    {
        add => StartAsync += value;
        remove => StartAsync -= value;
    }

    event AsyncEventHandler<EventArgs> ILanguageClient.StopAsync
    {
        add { }
        remove { }
    }

    static Process GetLSPProcess()
    {
        do
        {
            var process = Process
                .GetProcesses()
                .SingleOrDefault(p => p.ProcessName == "ReniLSP");
            if(process != null)
            {
                "Connecting...".Log();
                return process;
            }

            "Waiting...".Log();
            1.Seconds().Sleep();
        }
        while(true);
    }

    static Process CreateLSPProcess()
    {
        var process = new Process
        {
            StartInfo = new()
            {
                FileName = "a:/develop/Reni/dev/out/Debug/ReniLSP.exe"
                , RedirectStandardInput = true
                , RedirectStandardOutput = true
                , UseShellExecute = false
                , CreateNoWindow = true
            }
        };

        if(process.Start())
            return process;

        throw new("Process could not be started.");
    }

    event AsyncEventHandler<EventArgs> StartAsync;
}