using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace ReniLSP;

[UsedImplicitly]
sealed class MainWrapper
    : DumpableObject
        , ISemanticTokensFullHandler
        , ISemanticTokensDeltaHandler
        , ISemanticTokensRangeHandler
        , IDidOpenTextDocumentHandler
        , IDidChangeTextDocumentHandler
        , IDidCloseTextDocumentHandler
        , IDocumentFormattingHandler
        , IDidChangeConfigurationHandler
//, IDocumentHighlightHandler
{
    readonly DocumentFormattingHandlerWrapper FormattingHandler;
    readonly TextDocumentSyncHandlerWrapper DocumentHandler;
    readonly SemanticTokensHandlerWrapper SemanticHandler;
    readonly Handler Handler;

    public MainWrapper
    (
        ILogger<MainWrapper> logger
        , ITextDocumentLanguageServer server
        , IWorkspaceLanguageServer configurationHandler
    )
    {
        Handler = new(logger, server, configurationHandler);
        FormattingHandler = new(Handler);
        SemanticHandler = new(Handler);
        DocumentHandler = new(Handler);
    }

    void ICapability<DidChangeConfigurationCapability>.SetCapability
        (DidChangeConfigurationCapability capability, ClientCapabilities clientCapabilities)
        => DocumentHandler.SetCapability(capability, clientCapabilities);

    DocumentFormattingRegistrationOptions
        IRegistration<DocumentFormattingRegistrationOptions, DocumentFormattingCapability>
        .GetRegistrationOptions(DocumentFormattingCapability capability, ClientCapabilities clientCapabilities)
    {
        var result
            = ((IRegistration<DocumentFormattingRegistrationOptions, DocumentFormattingCapability>)FormattingHandler)
            .GetRegistrationOptions(capability, clientCapabilities);
        return result;
    }

    //DocumentHighlightRegistrationOptions
    //    IRegistration<DocumentHighlightRegistrationOptions, DocumentHighlightCapability>.GetRegistrationOptions
    //    (DocumentHighlightCapability capability, ClientCapabilities clientCapabilities)
    //    => ((IRegistration<DocumentHighlightRegistrationOptions, DocumentHighlightCapability>)DocumentHighlightHandler)
    //        .GetRegistrationOptions(capability, clientCapabilities);

    SemanticTokensRegistrationOptions IRegistration<SemanticTokensRegistrationOptions, SemanticTokensCapability>
        .GetRegistrationOptions(SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<SemanticTokensRegistrationOptions, SemanticTokensCapability>)SemanticHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentChangeRegistrationOptions
        IRegistration<TextDocumentChangeRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentChangeRegistrationOptions, TextSynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentCloseRegistrationOptions
        IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentOpenRegistrationOptions
        IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    async Task<Unit> IRequestHandler<DidChangeConfigurationParams, Unit>.Handle
        (DidChangeConfigurationParams request, CancellationToken cancellationToken)
    {
        Handler.DidChangeConfigurationHandlerImplementation(request);
        return await Unit.Task;
    }

    async Task<Unit> IRequestHandler<DidChangeTextDocumentParams, Unit>
        .Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        => await DocumentHandler.Handle(request, cancellationToken);

    async Task<Unit> IRequestHandler<DidCloseTextDocumentParams, Unit>
        .Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        => await DocumentHandler.Handle(request, cancellationToken);

    async Task<Unit> IRequestHandler<DidOpenTextDocumentParams, Unit>
        .Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        => await DocumentHandler.Handle(request, cancellationToken);

    async Task<TextEditContainer> IRequestHandler<DocumentFormattingParams, TextEditContainer>
        .Handle(DocumentFormattingParams request, CancellationToken cancellationToken)
        => await FormattingHandler.Handle(request, cancellationToken);

    async Task<SemanticTokensFullOrDelta> IRequestHandler<SemanticTokensDeltaParams, SemanticTokensFullOrDelta>
        .Handle(SemanticTokensDeltaParams request, CancellationToken token)
        => await SemanticHandler.Handle(request, token);

    async Task<SemanticTokens> IRequestHandler<SemanticTokensParams, SemanticTokens>
        .Handle(SemanticTokensParams request, CancellationToken token)
        => await SemanticHandler.Handle(request, token);

    async Task<SemanticTokens> IRequestHandler<SemanticTokensRangeParams, SemanticTokens>
        .Handle(SemanticTokensRangeParams request, CancellationToken token)
        => await SemanticHandler.Handle(request, token);
}