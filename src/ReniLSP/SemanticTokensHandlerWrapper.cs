using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ReniLSP;

sealed class SemanticTokensHandlerWrapper : SemanticTokensHandlerBase
{
    static readonly Container<SemanticTokenType> TokenTypes = new(
        "comment"
        , "keyword"
        , "number"
        , "string"
        , "variable"
        , "function"
        , "property"
    );

    public static readonly SemanticTokensRegistrationOptions.StaticOptions Capabilities = new()
    {
        Legend = new()
        {
            TokenModifiers = new("readonly")
            , TokenTypes = TokenTypes
        }
        , Full = new SemanticTokensCapabilityRequestFull
        {
            Delta = true
        }
        , Range = true
        , WorkDoneProgress = true
    };


    readonly Handler Handler;

    public SemanticTokensHandlerWrapper(Handler handler) => Handler = handler;

    protected override SemanticTokensRegistrationOptions
        CreateRegistrationOptions(SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
    {
        var c = clientCapabilities.TextDocument?.SemanticTokens;
        var cw = clientCapabilities.Workspace?.SemanticTokens;
        return new()
        {
            Legend = Capabilities.Legend
            , Full = Capabilities.Full
            , Range = Capabilities.Range
            , WorkDoneProgress = Capabilities.WorkDoneProgress
        }
            ;
    }

    protected override async Task Tokenize
    (
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken token
    )
    {
        Handler.Tokenize(builder, identifier);
        await Task.CompletedTask;
    }

    protected override async Task<SemanticTokensDocument>
        GetSemanticTokensDocument(ITextDocumentIdentifierParams @params, CancellationToken token)
        => await Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
}