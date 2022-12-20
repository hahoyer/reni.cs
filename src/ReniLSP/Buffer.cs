using System.Collections.Generic;
using System.Linq;
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
    readonly ValueCache<CompilerBrowser> CompilerCache;
    public Buffer() => CompilerCache = new(() => CompilerBrowser.FromText(Text, FileName));

    public void ApplyChanges(IEnumerable<TextDocumentContentChangeEvent> changes)
    {
        foreach(var change in changes)
        {
            change.Range.AssertIsNull();
            if(Text != change.Text)
                CompilerCache.IsValid = false;

            Text = change.Text;
        }
    }

    public void Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier)
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
                    builder.Push(range, type);
            }
        }
    }

    public TextEditContainer Format(ReniUI.Formatting.Configuration options)
    {
        var edits = options.Create()
            .GetEditPieces(CompilerCache.Value, CompilerCache.Value.Source.All)
            .Select(edit => new TextEdit { NewText = edit.Insert, Range = edit.Remove.GetRange() });
        return TextEditContainer.From(edits);
    }
}