using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Text;
using ReniUI;

namespace ReniVSIX;

class BufferContainer : DumpableObject
{
    protected readonly ITextBuffer Buffer;
    protected readonly ValueCache<CompilerBrowser> CompilerCache;

    protected BufferContainer(ITextBuffer buffer)
    {
        Buffer = buffer;
        CompilerCache = new(GetCompiler);
        Buffer.Changed += (sender, args) => CompilerCache.IsValid = false;
    }

    protected CompilerBrowser Compiler => CompilerCache.Value;

    CompilerBrowser GetCompiler()
    {
        (
                $"{DateTime.Now.DynamicShortFormat(false)}: " +
                $"Recompiling Version {Buffer.CurrentSnapshot.Version}..."
            )
            .Log();
        return CompilerBrowser.FromText(Buffer.CurrentSnapshot.GetText());
    }

    protected SnapshotSpan ToSpan(SourcePart sourcePart)
        => new(Buffer.CurrentSnapshot, new(sourcePart.Position, sourcePart.Length));
}