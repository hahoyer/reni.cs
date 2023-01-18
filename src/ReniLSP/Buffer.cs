using hw.DebugFormatter;
using hw.Helper;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReniUI;
using ReniUI.Formatting;

namespace ReniLSP;

sealed class Buffer : DumpableObject
{
    public string FileName;
    public string Text;

    readonly Handler Parent;
    readonly ValueCache<CompilerBrowser> CompilerCache;
    readonly ValueCache<IEnumerable<(Range Range, string Type)>> ItemsCache;

    public Buffer(Handler parent)
    {
        Parent = parent;
        CompilerCache = new(() => CompilerBrowser.FromText(Text
            , new() { ProcessErrors = true }
            , FileName));
        ItemsCache = new(() => GetItems().ToArray());
    }


    public void ApplyChanges(IEnumerable<TextDocumentContentChangeEvent> changes)
    {
        foreach(var change in changes)
        {
            change.Range.AssertIsNull();
            if(Text != change.Text)
                CompilerCache.IsValid = false;

            Text = change.Text;
        }

        Validate();
    }

    internal void Validate()
    {
        var issues = CompilerCache.Value.GuardedIssues.ToArray();
        if(issues.Any())
            Parent.PublishDiagnostics(FileName, issues);
    }

    public void Tokenize(SemanticTokensBuilder builder)
    {
        foreach(var item in ItemsCache.Value)
            builder.Push(item.Range, item.Type);
    }

    IEnumerable<(Range Range, string Type)> GetItems()
    {
        var nodes = CompilerCache
            .Value
            .Locate()
            .Where(item => item.IsComment || !item.IsWhiteSpace);

        foreach(var node in nodes)
        {
            var nodeTypes = node.GetTypes().ToArray();
            foreach(var line in node.SourcePart.Split("\n"))
            {
                var range = line.GetRange();
                foreach(var type in nodeTypes)
                    yield return (range, type);
            }
        }
    }

    public TextEditContainer Format(ReniUI.Formatting.Configuration options)
        => TextEditContainer
            .From(CompilerCache.Value.GetEditsForFormatting(options).Select(GetTextEdit));

    static TextEdit GetTextEdit(Edit edit)
        => new() { NewText = edit.Insert, Range = edit.Remove.GetRange() };
}