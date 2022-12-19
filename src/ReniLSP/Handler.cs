using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using ReniUI;
using ReniUI.Classification;

namespace ReniLSP;

sealed class Handler : DumpableObject
{
    static readonly Container<SemanticTokenType> TokenTypes = new("keyword", "comment", "string", "number", "variable"
        , "decorator");

    readonly ConcurrentDictionary<string, string> Buffers = new();
    readonly ILogger<TokenTarget> Logger;
    public Handler(ILogger<TokenTarget> logger) => Logger = logger;

    public static TextDocumentSyncRegistrationOptions GetDocumentRegistrationOptions()
        => new() { Change = TextDocumentSyncKind.Full };

    public static SemanticTokensRegistrationOptions GetSemanticTokensRegistrationOptions()
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

    public void Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier)
    {
        var text = Buffers[GetBufferKey(identifier.TextDocument)];
        var compiler = CompilerBrowser
            .FromText(text, GetBufferKey(identifier.TextDocument));
        var nodes = compiler
            .Locate()
            .Where(item => item.IsComment || !item.IsWhiteSpace);

        foreach(var node in nodes)
        foreach(var line in node.SourcePart.Split("\n"))
            builder.Push(GetRange(line), GetTokenTypeIndex(node));
    }

    static string GetBufferKey(TextDocumentIdentifier target) => target.Uri.GetFileSystemPath();

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
        var range = token.TextPosition;
        return new(range.start.LineNumber, range.start.ColumnNumber1 - 1, range.end.LineNumber
            , range.end.ColumnNumber1 - 1);
    }

    public static TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "reni");

    public void DidOpen(DidOpenTextDocumentParams request)
        => Buffers[GetBufferKey(request.TextDocument)] = request.TextDocument.Text;

    public void DidChange(DidChangeTextDocumentParams request)
        => ApplyChanges(GetBufferKey(request.TextDocument), request.ContentChanges);

    void ApplyChanges(string target, IEnumerable<TextDocumentContentChangeEvent> changes)
    {
        var text = Buffers[target];
        foreach(var change in changes)
        {
            change.Range.AssertIsNull();
            text = change.Text;
        }

        Buffers[target] = text;
    }

    public void DidClose(DidCloseTextDocumentParams request)
        => Buffers.TryRemove(GetBufferKey(request.TextDocument), out var _);
}