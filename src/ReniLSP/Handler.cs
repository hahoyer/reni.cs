using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Reni.Validation;

namespace ReniLSP;

sealed class Handler : DumpableObject
{
    internal readonly ITextDocumentLanguageServer Server;

    readonly ConcurrentDictionary<string, Buffer> Buffers = new();
    readonly ILogger<MainWrapper> Logger;
    ReniUI.Formatting.Configuration FormatOptions;

    public static DocumentHighlightRegistrationOptions HighlightOptions
        => new() { WorkDoneProgress = true };

    public static TextDocumentSyncRegistrationOptions DocumentOptions => new() { Change = TextDocumentSyncKind.Full };

    public static DocumentFormattingRegistrationOptions FormattingOptions
        => new()
        {
            WorkDoneProgress = true,
        };

    public Handler(ILogger<MainWrapper> logger, ITextDocumentLanguageServer server)
    {
        Logger = logger;
        Server = server;
    }

    public void Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier)
        => Buffers[identifier.TextDocument.GetKey()].Tokenize(builder);

    public static TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "reni");

    public void DidOpen(DidOpenTextDocumentParams request)
    {
        var fileName = request.TextDocument.GetKey();
        Buffers[fileName] = new(this)
        {
            FileName = fileName
            , Text = request.TextDocument.Text
            , IsValid = true
        };
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
        FormatOptions ??= CreateFormatOptions(null);

        if(options.TrimFinalNewlines)
            FormatOptions.LineBreakAtEndOfText = false;
        else if(options.InsertFinalNewline)
            FormatOptions.LineBreakAtEndOfText = true;
        else
            FormatOptions.LineBreakAtEndOfText = null;
        FormatOptions.IndentCount = options.TabSize;
    }

    public Container<JToken> Configuration(ConfigurationParams request)
    {
        NotImplementedMethod(request);
#pragma warning disable VSTHRD114 // Avoid returning a null Task
        return default;
#pragma warning restore VSTHRD114 // Avoid returning a null Task
    }

    public void DidChangeConfigurationHandlerImplementation(DidChangeConfigurationParams request)
    {
        FormatOptions = CreateFormatOptions(request);
        return;
    }

    static ReniUI.Formatting.Configuration CreateFormatOptions(DidChangeConfigurationParams request)
    {
        var settingsValues = request?.Settings?["reni"]?["formatting"];
        if(settingsValues?["list"] == null)
            return new()
            {
                EmptyLineLimit = null
                , MaxLineLength = null
                , AdditionalLineBreaksForMultilineItems = true
                , LineBreaksBeforeListToken = false
                , LineBreaksBeforeDeclarationToken = false
                , LineBreaksAtComplexDeclaration = true
            };


        return new()
        {
            EmptyLineLimit = settingsValues["EmptyLineLimit"]?.Value<int?>()
            , MaxLineLength = settingsValues["MaxLineLength"]?.Value<int?>()
            , AdditionalLineBreaksForMultilineItems
                = settingsValues["list"]!["AdditionalLineBreaksForMultilineItems"]!.Value<bool>()
            , LineBreaksBeforeListToken = settingsValues["list"]!["LineBreaksBeforeListToken"]!.Value<bool>()
            , LineBreaksBeforeDeclarationToken
                = settingsValues["list"]!["LineBreaksBeforeDeclarationToken"]!.Value<bool>()
            , LineBreaksAtComplexDeclaration = settingsValues["list"]!["LineBreaksAtComplexDeclaration"]!.Value<bool>()
        };
    }

    public void PublishDiagnostics(string fileName, IEnumerable<Issue> issues)
    {
        var result = new PublishDiagnosticsParams
        {
            Diagnostics = issues.Select(ToDiagnostic).ToArray()
            , Uri = DocumentUri.File(fileName)
        };

        Server.PublishDiagnostics(result);
    }

    static Diagnostic ToDiagnostic(Issue issue) => new()
    {
        Code = issue.Tag
        , Message = issue.Message
        , Range = issue.Position.GetRange()
        , Severity = DiagnosticSeverity.Error
    };
}