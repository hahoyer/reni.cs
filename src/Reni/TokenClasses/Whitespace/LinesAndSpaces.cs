using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class LinesAndSpaces : DumpableObject
{
    internal interface IConfiguration : LineGroup.IConfiguration
    {
        bool IsSeparatorRequired { get; }
        CommentGroup Prefix { get; }
    }

    [EnableDump]
    readonly LineGroup[] Lines;

    [EnableDump]
    readonly SourcePart Spaces;

    readonly IConfiguration Configuration;

    LinesAndSpaces(LineGroup[] lines, SourcePart spaces, IConfiguration configuration)
    {
        Lines = lines;
        Spaces = spaces;
        Configuration = configuration;
        Spaces.AssertIsNotNull();
    }

    int TargetLineCount
    {
        get
        {
            var maximalLineBreakCount = T(Configuration.EmptyLineLimit ?? Lines.Length, Lines.Length).Min();
            return T(Configuration.MinimalLineBreakCount, maximalLineBreakCount).Max();
        }
    }

    int TargetLineCountRespectingLineComment
        => T(TargetLineCount - (Configuration.Prefix?.Main.Type is ILine? 1 : 0), 0).Max();

    int LinesDelta => TargetLineCount - Lines.Length;

    /// <summary>
    ///     Indent cannot be handled with spaces, when there are neither lines nor spaces
    ///     since it would anchor the same source position as the edit for adding line breaks.
    /// </summary>
    bool IndentAtSpaces => Lines.Any() || Spaces.Length > 0;

    /// <summary>
    ///     Currently had no lines but now should have lines
    /// </summary>
    bool MakeLines => TargetLineCountRespectingLineComment > 0 && !Lines.Any();

    bool IsSeparatorRequired => Configuration.IsSeparatorRequired;

    internal static LinesAndSpaces Create(WhiteSpaceItem[] items, IConfiguration configuration)
    {
        (items != null && items.Any()).Assert();
        var groups = items.SplitAndTail(LineGroup.TailCondition);
        var tail = groups.Tail;
        var spaces = items.Last().SourcePart.End.Span(0);
        if(tail.Any())
            spaces = tail.First().SourcePart.Start.Span(tail.Last().SourcePart.End);
        return new(groups.Items.Select(items => new LineGroup(items)).ToArray(), spaces
            , configuration);
    }

    internal static LinesAndSpaces Create(SourcePosition anchor, IConfiguration configuration)
        => new(new LineGroup[0], anchor.Span(0), configuration);

    internal IEnumerable<Edit> GetEdits(int indent)
    {
        foreach(var edit in GetLineEdits())
            yield return edit;

        // when there are no lines, minimal line break count and probably indent should be ensured here 
        if(MakeLines)
        {
            var insert = "\n".Repeat(TargetLineCountRespectingLineComment) + " ".Repeat(IndentAtSpaces? 0 : indent);
            yield return new(Spaces.Start.Span(0), insert, "+minimalLineBreaks");
        }

        var targetSpacesCount
            = TargetLineCount > 0
                ? IndentAtSpaces
                    ? indent
                    : 0
                : IsSeparatorRequired
                    ? 1
                    : 0;

        var spacesEdit = GetSpaceEdits(targetSpacesCount);
        if(spacesEdit != null)
            yield return spacesEdit;
    }

    Edit GetSpaceEdits(int targetCount)
    {
        var delta = targetCount - (Spaces?.Length ?? 0);
        if(delta == 0)
            return null;

        Spaces.AssertIsNotNull();

        return new
        (
            Spaces.End.Span(T(delta, 0).Min()),
            " ".Repeat(T(delta, 0).Max()),
            "+/-spaces"
        );
    }

    IEnumerable<Edit> GetLineEdits()
    {
        if(!Lines.Any())
            yield break;

        switch(LinesDelta)
        {
            case < 0:
            {
                var start = Lines[0].SourcePart.Start;
                var end = -LinesDelta < Lines.Length
                    ? Lines[-LinesDelta].Main.SourcePart.Start
                    : Lines[-LinesDelta - 1].Main.SourcePart.End;

                yield return new(start.Span(end), "", "-extra Linebreaks");
                break;
            }
            case > 0:
                foreach(var edit in Lines.SelectMany(item => item.GetEdits()))
                    yield return edit;
                yield return new(Lines.Last().SourcePart.End.Span(0), "\n".Repeat(LinesDelta), "+linebreaks");
                break;
            case 0:
                foreach(var edit in Lines.SelectMany(item => item.GetEdits()))
                    yield return edit;
                break;
        }
    }
}