using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

sealed class TextDocumentSyncHandlerWrapper : TextDocumentSyncHandlerBase
{
    readonly Handler Handler;
    public TextDocumentSyncHandlerWrapper(Handler handler) => Handler = handler;

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => Handler.GetTextDocumentAttributes(uri);

    public override async Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        Handler.DidOpen(request);
        return await Unit.Task;
    }

    public override async Task<Unit> Handle
        (DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        Handler.DidChange(request);
        return await Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public override async Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        Handler.DidClose(request);
        return await Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions
        (SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => Handler.DocumentOptions;
}