using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace ReniVSIX;

[ContentType(Constants.LanguageName)]
[Export(typeof(ILanguageClient))]
public class LanguageClient : ILanguageClient
{
    public LanguageClient() => Debugger.Launch();

    async Task<Connection> ILanguageClient.ActivateAsync(CancellationToken token)
    {
        await Task.Yield();

        var info = new ProcessStartInfo();
        info.FileName = "a:/delvelop/Reni/dev/out/Debug/ReniLSP.exe";
        info.Arguments = "bar";
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        info.UseShellExecute = false;
        info.CreateNoWindow = true;

        var process = new Process();
        process.StartInfo = info;

        return process.Start()
            ? new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream)
            : null;
    }


    IEnumerable<string> ILanguageClient.ConfigurationSections => null;

    IEnumerable<string> ILanguageClient.FilesToWatch => null;

    object ILanguageClient.InitializationOptions => null;
    string ILanguageClient.Name => "Bar Language Extension";

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

    event AsyncEventHandler<EventArgs> StartAsync;
}