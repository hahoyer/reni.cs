using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    class LinesAndSpaces : DumpableObject
    {
        internal interface IConfiguration : LineGroup.IConfiguration { }

        [EnableDump]
        readonly LineGroup[] Lines;

        [EnableDump]
        readonly SourcePart Spaces;

        readonly int TargetLineCount;
        readonly int LinesDelta;

        LinesAndSpaces(LineGroup[] lines, SourcePart spaces, IConfiguration configuration)
        {
            Lines = lines;
            Spaces = spaces;
            Spaces.AssertIsNotNull();

            var maximalLineBreakCount = T(configuration.EmptyLineLimit ?? Lines.Length, Lines.Length).Min();
            TargetLineCount = T(configuration.MinimalLineBreakCount, maximalLineBreakCount).Max();
            LinesDelta = TargetLineCount - Lines.Length;
        }

        internal static LinesAndSpaces Create(WhiteSpaceItem[] items, IConfiguration configuration)
        {
            (items != null && items.Any()).Assert();
            var groups = items.SplitAndTail(LineGroup.TailCondition);
            var tail = groups.Tail;
            var spaces = items.Last().SourcePart.End.Span(0);
            if(tail.Any())
                spaces = tail.First().SourcePart.Start.Span(tail.Last().SourcePart.End);
            return new(groups.Items.Select(items => new LineGroup(items, configuration)).ToArray(), spaces
                , configuration);
        }

        internal static LinesAndSpaces Create(SourcePosition anchor, CommentGroup.IConfiguration configuration)
            => new(new LineGroup[0], anchor.Span(0), configuration);

        internal IEnumerable<Edit> GetEdits(bool isSeparatorRequired, int indent)
        {
            foreach(var edit in GetLineEdits())
                yield return edit;

            var actualIndent = TargetLineCount > 0? indent : 0;
            var indentAtSpaces = Spaces.Length == 0? 0 : actualIndent;
            var indentAtEmptyLineBranch = Spaces.Length == 0? actualIndent : 0;


            // when there are no lines, minimal line break count and probably indent should be ensured here 
            if(!Lines.Any() && TargetLineCount > 0)
            {
                var insert = "\n".Repeat(TargetLineCount) + " ".Repeat(indentAtEmptyLineBranch);
                yield return new(Spaces.Start.Span(0), insert, "+minimalLineBreaks");
            }

            (!isSeparatorRequired || TargetLineCount == 0).Assert();
            var spacesEdit = GetSpaceEdits((isSeparatorRequired? 1 : 0) + indentAtSpaces);
            if(spacesEdit != null)
                yield return spacesEdit;
        }

        Edit GetSpaceEdits(int targetCount)
        {
            if(Spaces == null)
            {
                (targetCount == 0).Assert();
                return null;
            }

            var delta = targetCount - Spaces.Length;
            if(delta == 0)
                return null;

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
                    NotImplementedFunction();
                    break;
                case 0:
                    foreach(var edit in Lines.SelectMany(item => item.GetEdits()))
                        yield return edit;
                    break;
            }
        }
    }
}