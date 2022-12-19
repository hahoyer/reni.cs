using System.Collections.Concurrent;
using System.Threading.Tasks;
using hw.DebugFormatter;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace ReniLSP;

sealed class Handler : DumpableObject
{
    static readonly Container<SemanticTokenType> TokenTypes = new(
        "comment"
        , "keyword"
        , "number"
        , "string"
        , "variable"
    );

    readonly ConcurrentDictionary<string, Buffer> Buffers = new();
    readonly ILogger<TokenTarget> Logger;

    public static TextDocumentSyncRegistrationOptions DocumentOptions => new() { Change = TextDocumentSyncKind.Full };

    public static SemanticTokensRegistrationOptions SemanticTokensOptions
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

    public static DocumentFormattingRegistrationOptions FormattingOptions
        => new()
        {
            WorkDoneProgress = true,
        };

    public Handler(ILogger<TokenTarget> logger) => Logger = logger;

    public void Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier)
        => Buffers[identifier.TextDocument.GetKey()].Tokenize(builder, identifier);

    public static TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "reni");

    public void DidOpen(DidOpenTextDocumentParams request)
    {
        var fileName = request.TextDocument.GetKey();
        Buffers[fileName] = new() { FileName = fileName, Text = request.TextDocument.Text };
    }

    public void DidChange(DidChangeTextDocumentParams request)
        => Buffers[request.TextDocument.GetKey()].ApplyChanges(request.ContentChanges);

    public void DidClose(DidCloseTextDocumentParams request)
        => Buffers.TryRemove(request.TextDocument.GetKey(), out var _);

    public Task<TextEditContainer> Format(DocumentFormattingParams request)
        => Task.FromResult(Buffers[request.TextDocument.GetKey()].Format(request.Options));
}