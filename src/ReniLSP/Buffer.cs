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

    internal bool IsValid
    {
        set
        {
            if(!value)
                return;

            var issues = CompilerCache.Value.GuardedIssues.ToArray();
            Parent.PublishDiagnostics(FileName, issues);
        }
    }

    public Buffer(Handler parent)
    {
        Parent = parent;
        CompilerCache = new(() => CompilerBrowser.FromText(Text
            , new() { ProcessErrors = true , Semantics = true}
            , FileName));
        ItemsCache = new(() => GetItems().ToArray());
    }


    public void ApplyChanges(IEnumerable<TextDocumentContentChangeEvent> changes)
    {
        lock(CompilerCache)
        {
            foreach(var change in changes)
            {
                change.Range.AssertIsNull();
                if(Text != change.Text)
                    CompilerCache.IsValid = false;

                Text = change.Text;
            }

            ItemsCache.IsValid = false;
            IsValid = true;
        }
    }

    public void Tokenize(SemanticTokensBuilder builder)
    {
        var items = ItemsCache.Value;
        foreach(var item in items)
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
            var nodeTypes = node.LSPTypes.ToArray();
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