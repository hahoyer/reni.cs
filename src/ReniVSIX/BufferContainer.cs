using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Text;
using ReniUI;

namespace ReniVSIX
{
    class BufferContainer : DumpableObject
    {
        protected readonly ITextBuffer Buffer;
        protected readonly ValueCache<CompilerBrowser> CompilerCache;

        protected BufferContainer(ITextBuffer buffer)
        {
            Buffer = buffer;
            CompilerCache = new ValueCache<CompilerBrowser>(GetCompiler);
            Buffer.Changed += (sender, args) => CompilerCache.IsValid = false;
        }

        protected CompilerBrowser Compiler => CompilerCache.Value;
        CompilerBrowser GetCompiler() => CompilerBrowser.FromText(Buffer.CurrentSnapshot.GetText());

        protected SnapshotSpan ToSpan(SourcePart sourcePart)
            => new SnapshotSpan(Buffer.CurrentSnapshot, new Span(sourcePart.Position, sourcePart.Length));
    }
}