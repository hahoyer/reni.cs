using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    class LinesAndSpaces: DumpableObject
    {
        [EnableDump]
        readonly LineGroup[] Lines;

        [EnableDump]
        readonly WhiteSpaceItem[] Spaces;

        LinesAndSpaces(LineGroup[] lines, WhiteSpaceItem[] spaces)
        {
            Lines = lines;
            Spaces = spaces;
        }

        public static LinesAndSpaces Create (WhiteSpaceItem[] items, CommentGroup.IConfiguration configuration)
        {
            var groups = items.SplitAndTail(LineGroup.TailCondition);
            return new(groups.Items.Select(items => new LineGroup(items, configuration)).ToArray(), groups.Tail);

        }

        internal IEnumerable<Edit> GetEdits(int indent, bool isSeparatorRequired, SourcePosition anchor)
        {
            if(indent > 0)
            {
                NotImplementedMethod(indent, isSeparatorRequired, anchor);
                return default;
            }

            if(Lines.Any() && Spaces.Any())
            {
                NotImplementedMethod(indent, isSeparatorRequired, anchor);
                return default;
            }

            var lineEdits = Lines.GetLineEdits(indent, !Spaces.Any() && isSeparatorRequired).ToArray();
            var spacesEdits = Spaces.GetSpaceEdits(isSeparatorRequired).ToArray();

            var separatorEdit = !Lines.Any() && !Spaces.Any() && isSeparatorRequired
                ? new Edit[] { new(anchor.Span(0), " ", "+separator") }
                : new Edit[0];

            return T(lineEdits, spacesEdits, separatorEdit).ConcatMany();
        }
    }
}