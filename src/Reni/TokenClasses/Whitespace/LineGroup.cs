using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace;

sealed class LineGroup : DumpableObject
{
    [EnableDump]
    internal readonly WhiteSpaceItem LineBreak;

    [EnableDump]
    readonly int SpaceCount;

    internal LineGroup(int spaceCount, WhiteSpaceItem lineBreak)
    {
        LineBreak = lineBreak;
        SpaceCount = spaceCount;
    }

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    internal SourcePart SourcePart => SpacesPart.Start.Span(LineBreak.SourcePart.End);

    internal SourcePart SpacesPart => LineBreak.SourcePart.Start.Span(-SpaceCount);

    internal IEnumerable<Edit> GetEdits()
    {
        if(SpaceCount > 0)
            yield return new(SpacesPart, "", "-spaces");
    }
}