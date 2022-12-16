using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReniUI;
using ReniUI.Classification;

namespace ReniLSP;

[UsedImplicitly]
sealed class TokenTarget : SemanticTokensHandlerBase
{
    static readonly Container<SemanticTokenType> TokenTypes = new("keyword", "comment", "string", "number", "variable"
        , "decorator");

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions
        (SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
        => GetSemanticTokensRegistrationOptions();

    protected override Task Tokenize
    (
        SemanticTokensBuilder builder
        , ITextDocumentIdentifierParams identifier
        , CancellationToken cancellationToken
    )
    {
        var compiler = CompilerBrowser
            .FromFile(DocumentUri.GetFileSystemPath(identifier).AssertNotNull());
        var nodes = compiler
            .Locate()
            .Where(item => item.IsComment || !item.IsWhiteSpace);

        foreach(var item in nodes)
        foreach(var line in item.SourcePart.Split("\n"))
            builder.Push(GetRange(line), GetTokenTypeIndex(item));
        return Task.CompletedTask;
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument
        (ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
        => Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));

    static SemanticTokenType? GetTokenTypeIndex(Item token)
        => token.IsComment
            ? SemanticTokenType.Comment
            : token.IsBraceLike
                ? SemanticTokenType.Keyword
                : token.IsIdentifier
                    ? SemanticTokenType.Variable
                    : token.IsKeyword
                        ? SemanticTokenType.Keyword
                        : token.IsText
                            ? SemanticTokenType.String
                            : token.IsNumber
                                ? SemanticTokenType.Number
                                : SemanticTokenType.Namespace;

    static Range GetRange(SourcePart token)
    {
        var r = token.TextPosition;
        return new(r.start.LineNumber, r.start.ColumnNumber - 1, r.end.LineNumber, r.end.ColumnNumber - 1);
    }

    static SemanticTokensRegistrationOptions GetSemanticTokensRegistrationOptions()
        => new()
        {
            Legend = new()
            {
                TokenModifiers = new("readonly")
                , TokenTypes = TokenTypes
            }
            , Full = true
            , Range = true
        };
}