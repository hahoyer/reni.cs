using System.Collections.Concurrent;
using System.Threading.Tasks;
using hw.DebugFormatter;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
    readonly ILogger<MainWrapper> Logger;
    ReniUI.Formatting.Configuration FormatOptions;

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

    public Handler(ILogger<MainWrapper> logger) => Logger = logger;

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
    {
        SetFormattingOptions(request.Options);
        return Task.FromResult(Buffers[request.TextDocument.GetKey()].Format(FormatOptions));
    }

    void SetFormattingOptions(FormattingOptions options)
    {
        if(options.TrimFinalNewlines)
            FormatOptions.LineBreakAtEndOfText = false;
        else if(options.InsertFinalNewline)
            FormatOptions.LineBreakAtEndOfText = true;
        else
            FormatOptions.LineBreakAtEndOfText = null;
        FormatOptions.IndentCount = options.TabSize;
    }

    public Task<Container<JToken>> Configuration(ConfigurationParams request)
    {
        NotImplementedMethod(request);
        return default;
    }

    public void DidChangeConfigurationHandlerImplementation(DidChangeConfigurationParams request)
    {
        var settingsValues = request.Settings?["reni"]?["formatting"];
        if(settingsValues?["list"] == null)
            return;

        FormatOptions = new()
        {
            EmptyLineLimit = settingsValues["EmptyLineLimit"].Value<int?>()
            , MaxLineLength = settingsValues["MaxLineLength"].Value<int?>()
            , AdditionalLineBreaksForMultilineItems
                = settingsValues["list"]["AdditionalLineBreaksForMultilineItems"].Value<bool>()
            , LineBreaksBeforeListToken = settingsValues["list"]["LineBreaksBeforeListToken"].Value<bool>()
            , LineBreaksBeforeDeclarationToken
                = settingsValues["list"]["LineBreaksBeforeDeclarationToken"].Value<bool>()
            , LineBreaksAtComplexDeclaration = settingsValues["list"]["LineBreaksAtComplexDeclaration"].Value<bool>()
        };
        return;
    }
}