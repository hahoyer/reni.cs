using System.Threading;
using System.Threading.Tasks;
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

    protected override Task Tokenize
    (
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken
    )
    {
        Handler.Tokenize(builder, identifier);
        return Task.CompletedTask;
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument
        (ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
        => Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
}