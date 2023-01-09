using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using ReniLSP;

namespace ReniLSPVSIX;

[ContentType(Constants.LanguageName)]
[Export(typeof(ILanguageClient))]
public sealed class LanguageClient : ILanguageClient
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
        get { yield return "reni"; }
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

    event AsyncEventHandler<EventArgs> StartAsync;
}