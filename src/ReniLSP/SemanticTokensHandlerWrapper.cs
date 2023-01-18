using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

sealed class SemanticTokensHandlerWrapper : SemanticTokensHandlerBase
{
    readonly Handler Handler;

    public SemanticTokensHandlerWrapper(Handler handler) => Handler = handler;

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions
        (SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
        => Handler.SemanticTokensOptions;

    protected override async Task Tokenize
    (
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken
    )
    {
        Handler.Tokenize(builder, identifier);
        await Task.CompletedTask;
    }

    protected override async Task<SemanticTokensDocument> GetSemanticTokensDocument
        (ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
        => await Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
}