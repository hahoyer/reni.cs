using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    class LinesAndSpaces : DumpableObject
    {
        internal interface IConfiguration : LineGroup.IConfiguration
        {
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
            if(Lines.Any() && Spaces.Length > 0)
            {
                NotImplementedMethod(isSeparatorRequired, indent);
                return default;
            }

            var addLineBreaks = Configuration.MinimalLineBreakCount;

            var lineEdits = Lines.GetLineEdits().ToArray();

            var addLineBreaksEdit = addLineBreaks == 0 || Lines.Any()
                ? new Edit[0]
                : new Edit[]
                    { new(Spaces.Start.Span(0), "\n".Repeat(addLineBreaks), "+minimalLineBreaks") };

            (!isSeparatorRequired || addLineBreaks == 0).Assert();

            var spacesCount = (isSeparatorRequired? 1 : 0) + (addLineBreaks > 0? indent : 0);
            var spacesEdits = Spaces.GetSpaceEdits(spacesCount).ToArray();
            return T(lineEdits, addLineBreaksEdit, spacesEdits).ConcatMany();
        }

    }
}