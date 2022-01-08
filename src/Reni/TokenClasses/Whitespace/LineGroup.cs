using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses.Whitespace;

sealed class LineGroup : DumpableObject
{
    internal interface IConfiguration
    {
        int? EmptyLineLimit { get; }
        SeparatorRequests SeparatorRequests { get; }
        int MinimalLineBreakCount { get; }
    }

    [EnableDump]
    internal readonly SourcePart SourcePart;

    [EnableDump]
    internal readonly WhiteSpaceItem Main;

    [EnableDump]
    readonly int Spaces;

    internal LineGroup(IEnumerable<WhiteSpaceItem> allItems)
    {
        var groups = allItems
            .GroupBy(TailCondition)
            .ToDictionary(item => item.Key, item => item.ToArray());

        groups.TryGetValue(false, out var items);
        groups.TryGetValue(true, out var tails);

        Main = tails.AssertNotNull().Single();

        Spaces = items?.Length ?? 0;
        SourcePart = allItems.Select(item => item.SourcePart).Combine();
    }

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    internal static(LineGroup[], WhiteSpaceItem[]) Create(WhiteSpaceItem[] items, IConfiguration configuration)
    {
        var groups = items.SplitAndTail(TailCondition);
        return (groups.Items.Select(items => new LineGroup(items)).ToArray()
            , (groups.Tail));
    }

    internal static bool TailCondition(WhiteSpaceItem item) => item.Type is IVolatileLineBreak;

    internal IEnumerable<Edit> GetEdits()
    {
        if(Spaces > 0)
            yield return new(Main.SourcePart.Start.Span(-Spaces), "", "-spaces");
    }
}