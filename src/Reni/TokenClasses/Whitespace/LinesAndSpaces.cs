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

        readonly IConfiguration Configuration;

        LinesAndSpaces(LineGroup[] lines, SourcePart spaces, IConfiguration configuration)
        {
            Lines = lines;
            Spaces = spaces;
            Spaces.AssertIsNotNull();
            Configuration = configuration;
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
            foreach(var edit in Lines.GetLineEdits())
                yield return edit;

            var minimalLineBreakCount = Configuration.MinimalLineBreakCount;

            // when there are no lines, minimal line break count should be ensured here 
            if(!Lines.Any() && minimalLineBreakCount > 0)
                yield return new(Spaces.Start.Span(0), "\n".Repeat(minimalLineBreakCount), "+minimalLineBreaks");

            (!isSeparatorRequired || minimalLineBreakCount == 0).Assert();
            var spacesCount = (isSeparatorRequired? 1 : 0) + (minimalLineBreakCount > 0? indent : 0);
            var spacesEdit = Spaces.GetSpaceEdits(spacesCount);
            if(spacesEdit != null)
                yield return spacesEdit;
        }
    }
}