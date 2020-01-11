using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    sealed class SourcePartEdit : DumpableObject, ISourcePartProxy, ISourcePartEdit
    {
        [EnableDump]
        SourcePosn Anchor;
        SourcePart[] LineBreakPrefixes;
        SourcePart[] RelevantSpaces;
        [EnableDumpExcept(exception: false)]
        bool HasCommentLineBreak;

        SourcePart ISourcePartProxy.All
            => (LineBreakPrefixes.Any() ? LineBreakPrefixes.First().Start : Anchor)
                .Span(Anchor + (RelativeSpacePositions.Any() ? RelativeSpacePositions.Last() : 0));

        [EnableDump]
        int[] RelativeSpacePositions => RelevantSpaces.Select(part => part.End - Anchor).ToArray();

        internal string OrientationDump => Anchor.GetDumpAroundCurrent(dumpWidth: 5);

        internal IEnumerable<Edit> ConvertToEdits(ContextForConvertToEdits context)
        {
            var e = new EditConverter
                (context, LineBreakPrefixes, Anchor, RelativeSpacePositions, HasCommentLineBreak);
            return e.Value;
        }

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;

        public static ISourcePartEdit[] Create(IToken token)
        {
            if(token == null)
                return new ISourcePartEdit[0];

            return token
                .PrecededWith
                .Split(item => item.IsComment(), true)
                .Select(items => CreateCommentGroup(items, token.SourcePart().Start))
                .ToArray<ISourcePartEdit>();
        }

        static SourcePartEdit CreateCommentGroup(IEnumerable<IItem> items, SourcePosn tokenStart)
        {
            var lineGroups = items.Split(item => item.IsLineBreak(), false);
            var resultItem = new SourcePartEdit
            {
                Anchor = items.LastOrDefault(item => item.IsComment() || item.IsLineBreak())?.SourcePart.End ?? tokenStart,
                RelevantSpaces = GetSpaces(lineGroups.LastOrDefault()).ToArray(),
                LineBreakPrefixes = lineGroups.Select(GetLineBreakPrefix).ToArray(),
                HasCommentLineBreak = GetHasCommentLineBreak(items.FirstOrDefault(i => i.IsComment())),
            };
            return resultItem;
        }

        static SourcePart GetLineBreakPrefix(IEnumerable<IItem> target)
        {
            var start = target.First(item => item.IsLineBreak() || item.IsWhiteSpace());
            var end = target.Single(item => item.IsLineBreak());
            return start.SourcePart.Start.Span(end.SourcePart.End);
        }

        static IEnumerable<SourcePart> GetSpaces(IEnumerable<IItem> target)
            => target == null
                ? new SourcePart[0]
                : target.Where(item => item.IsWhiteSpace()).Select(item => item.SourcePart);

        static bool GetHasCommentLineBreak(IItem target)
            => target != null && Lexer.IsLineComment(target);
    }
}