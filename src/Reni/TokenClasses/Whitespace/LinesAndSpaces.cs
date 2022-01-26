using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class LinesAndSpaces : DumpableObject
{
    internal interface IConfiguration
    {
        int? EmptyLineLimit { get; }
        SeparatorRequests SeparatorRequests { get; }
        int MinimalLineBreakCount { get; }
        string LineBreakString { get; }
    }

    [EnableDump]
    readonly SourcePart[] Lines;

    [EnableDump]
    readonly SourcePart Spaces;

    readonly IConfiguration Configuration;
    readonly bool IsLast;
    readonly IItemType Predecessor;

    internal LinesAndSpaces
    (
        SourcePart[] lines
        , SourcePart spaces
        , IConfiguration configuration
        , IItemType predecessor
        , bool isLast
    )
    {
        Lines = lines;
        Spaces = spaces;
        Configuration = configuration;
        IsLast = isLast;
        Spaces.AssertIsNotNull();
        Predecessor = predecessor;
    }

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    /// <summary>
    ///     Target line count respects everything:
    ///     the current number of lines (including any preceding line comment),
    ///     the empty line limit of configuration if set and
    ///     if it is the last lines and spaces-group, the minimal line break count of configuration
    /// </summary>
    int TargetLineCount
    {
        get
        {
            var maximalLineBreakCount = Lines.Length;
            if(Predecessor is ILine)
                maximalLineBreakCount++;
            if(Configuration.EmptyLineLimit != null && Configuration.EmptyLineLimit.Value < maximalLineBreakCount)
                maximalLineBreakCount = Configuration.EmptyLineLimit.Value;

            var minimalLineBreakCount = IsLast? Configuration.MinimalLineBreakCount : 0;
            return T(minimalLineBreakCount, maximalLineBreakCount).Max();
        }
    }

    [EnableDump]
    SourcePart SourcePart => (Lines.FirstOrDefault() ?? Spaces).Start.Span(Spaces.End);

    /// <summary>
    ///     Actual target line count is like <see cref="TargetLineCount" /> but ignoring a preceding line comment
    /// </summary>
    int ActualTargetLineCount
        => T(TargetLineCount - (Predecessor is ILine? 1 : 0), 0).Max();

    bool WillHaveLineBreak => TargetLineCount > 0;

    bool IsSeparatorRequired
        => Configuration.SeparatorRequests.Get(Predecessor == null, IsLast) &&
            Predecessor is not ILine;

    string LineBreak => Configuration.LineBreakString;

    internal IEnumerable<Edit> GetEdits(int indent)
    {
        var targetSpacesCount
            = WillHaveLineBreak
                ? indent
                : IsSeparatorRequired
                    ? 1
                    : 0;

        var insert = LineBreak.Repeat(ActualTargetLineCount) + " ".Repeat(targetSpacesCount);
        return Edit.Create(SourcePart, insert);
    }
}