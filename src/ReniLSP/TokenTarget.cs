using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

[UsedImplicitly]
sealed class TokenTarget
    : DumpableObject
        , ISemanticTokensFullHandler
        , ISemanticTokensDeltaHandler
        , ISemanticTokensRangeHandler
        , IDidOpenTextDocumentHandler
        , IDidChangeTextDocumentHandler
        , IDidCloseTextDocumentHandler
        , IDocumentFormattingHandler
{
    readonly DocumentFormattingHandlerWrapper FormattingHandler;
    readonly TextDocumentSyncHandlerWrapper DocumentHandler;
    readonly SemanticTokensHandlerWrapper SemanticHandler;

    public TokenTarget(ILogger<TokenTarget> logger)
    {
        Handler handler = new(logger);
        FormattingHandler = new(handler);
        SemanticHandler = new(handler);
        DocumentHandler = new(handler);
    }

    DocumentFormattingRegistrationOptions
        IRegistration<DocumentFormattingRegistrationOptions, DocumentFormattingCapability>
        .GetRegistrationOptions(DocumentFormattingCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<DocumentFormattingRegistrationOptions, DocumentFormattingCapability>)FormattingHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    SemanticTokensRegistrationOptions IRegistration<SemanticTokensRegistrationOptions, SemanticTokensCapability>
        .GetRegistrationOptions(SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<SemanticTokensRegistrationOptions, SemanticTokensCapability>)SemanticHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentChangeRegistrationOptions
        IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>
        .GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>
        .GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

    TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>
        .GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => ((IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>)DocumentHandler)
            .GetRegistrationOptions(capability, clientCapabilities);

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
        .Handle(SemanticTokensDeltaParams request, CancellationToken cancellationToken)
        => await ((IRequestHandler<SemanticTokensDeltaParams, SemanticTokensFullOrDelta>)SemanticHandler)
            .Handle(request, cancellationToken);

    async Task<SemanticTokens> IRequestHandler<SemanticTokensParams, SemanticTokens>
        .Handle(SemanticTokensParams request, CancellationToken cancellationToken)
        => await ((IRequestHandler<SemanticTokensParams, SemanticTokens>)SemanticHandler)
            .Handle(request, cancellationToken);

    async Task<SemanticTokens> IRequestHandler<SemanticTokensRangeParams, SemanticTokens>
        .Handle(SemanticTokensRangeParams request, CancellationToken cancellationToken)
        => await ((IRequestHandler<SemanticTokensRangeParams, SemanticTokens>)SemanticHandler)
            .Handle(request, cancellationToken);
}