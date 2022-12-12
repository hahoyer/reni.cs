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
[RunOnContext(RunningContext.RunOnHost)]
public class LanguageClient : ILanguageClient
{
    internal static LanguageClient Instance { get; set; }
    public LanguageClient() => Instance = this;

    async Task<Connection> ILanguageClient.ActivateAsync(CancellationToken token)
    {
        await Task.Yield();

        var info = new ProcessStartInfo();
        info.FileName = "a:/develop/Reni/dev/out/Debug/ReniLSP.exe";
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

    static async Task<Connection> Activate()
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

    event AsyncEventHandler<EventArgs> StartAsync;
}