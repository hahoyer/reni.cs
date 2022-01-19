using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Text;
using ReniUI;

namespace ReniVSIX;

sealed class BufferContainer : DumpableObject
{
    internal readonly ITextBuffer Buffer;
    readonly ValueCache<CompilerBrowser> CompilerCache;

    internal BufferContainer(ITextBuffer buffer)
    {
        Buffer = buffer;
        CompilerCache = new(GetCompiler);
        Buffer.Changed += (sender, args) => CompilerCache.IsValid = false;
    }

    internal CompilerBrowser Compiler => CompilerCache.Value;

    CompilerBrowser GetCompiler()
    {
        (
                $"{DateTime.Now.DynamicShortFormat(false)}: " +
                $"Recompiling Version {Buffer.CurrentSnapshot.Version}..."
            )
            .Log();
        return CompilerBrowser.FromText(Buffer.CurrentSnapshot.GetText());
    }

    internal SnapshotSpan ToSpan(SourcePart sourcePart)
        => new(Buffer.CurrentSnapshot, new(sourcePart.Position, sourcePart.Length));
}