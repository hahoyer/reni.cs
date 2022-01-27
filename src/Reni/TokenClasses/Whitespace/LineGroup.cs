using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class LineGroup : DumpableObject
{
    internal interface IConfiguration
    {
        int? EmptyLineLimit { get; }
        SeparatorRequests SeparatorRequests { get; }
        int MinimalLineBreakCount { get; }
        string LineBreakString { get; }
    }

    [EnableDump]
    readonly int Lines;

    readonly IConfiguration Configuration;
    readonly bool IsLast;
    readonly IItemType Predecessor;

    /// <summary>
    ///     When there is no line limit by configuration it is the number of lines in this group.
    ///     Otherwise it is the minimum of the line limit by configuration
    ///     - reduced by one if the predecessor is a line comment - and number of lines.
    /// </summary>
    /// <remarks>Can be -1 if predecessor is a line comment and configuration line limit is 0.</remarks>
    [UsedImplicitly]
    readonly int LineBreaksToKeep;

    /// <summary>
    ///     When this line group belongs to a comment group it is 0.
    ///     Otherwise it is the minimal line break count from configuration,
    ///     reduced by one if the predecessor is a line comment.
    /// </summary>
    /// <remarks>Can be -1 if it is last, predecessor is a line comment and minimal line break count by configuration is 0.</remarks>
    [UsedImplicitly]
    readonly int MinimalLineBreakCount;

    /// <summary>
    ///     Target line count respects everything:
    ///     the current number of lines (including any preceding line comment),
    ///     the empty line limit of configuration if set and
    ///     if it is the last lines-and-spaces-group, the minimal line break count of configuration
    /// </summary>
    readonly int TargetLineCount;

    [EnableDump]
    SourcePart SourcePart;

    internal LineGroup
    (
        SourcePart[] lines
        , SourcePart spaces
        , IConfiguration configuration
        , IItemType predecessor
        , bool isLast
    )
    {
        spaces.AssertIsNotNull();
        SourcePart = (lines.FirstOrDefault() ?? spaces).Start.Span(spaces.End);
        Lines = lines.Length;
        Configuration = configuration;
        IsLast = isLast;
        Predecessor = predecessor;
        var predecessorLine = predecessor is ILine? 1 : 0;
        MinimalLineBreakCount = IsLast? Configuration.MinimalLineBreakCount - predecessorLine : 0;
        LineBreaksToKeep = Configuration.EmptyLineLimit == null ||
            Configuration.EmptyLineLimit.Value - predecessorLine >= Lines
                ? Lines
                : (Configuration.EmptyLineLimit - predecessorLine).Value;
        TargetLineCount = T(MinimalLineBreakCount, LineBreaksToKeep, 0).Max();
    }

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    bool WillHaveLineBreak => Predecessor is ILine || TargetLineCount > 0;

    bool IsSeparatorRequired => Configuration.SeparatorRequests.Get(Predecessor == null, IsLast);

    internal IEnumerable<Edit> GetEdits(int indent)
    {
        var insert = Configuration.LineBreakString.Repeat(TargetLineCount) + " ".Repeat(GetTargetSpacesCount(indent));
        return Edit.Create(SourcePart, insert);
    }

    int GetTargetSpacesCount(int indent)
        => WillHaveLineBreak
            ? indent
            : IsSeparatorRequired
                ? 1
                : 0;
}