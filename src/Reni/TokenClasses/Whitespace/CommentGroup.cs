using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    class CommentGroup : DumpableObject
    {
        internal interface IConfiguration : LinesAndSpaces.IConfiguration { }

        [EnableDump]
        readonly LinesAndSpaces LinesAndSpaces;

        [EnableDump]
        readonly WhiteSpaceItem Main;

        CommentGroup(IEnumerable<WhiteSpaceItem> allItems, IConfiguration configuration)
        {
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            Main = groups[true].Single();

            groups.TryGetValue(false, out var items);
            LinesAndSpaces = items == null || !items.Any()
                ? LinesAndSpaces.Create(Main.SourcePart.Start, configuration)
                : LinesAndSpaces.Create(items, configuration);
        }

        [DisableDump]
        internal bool IsSeparatorRequired => Main.Type.IsSeparatorRequired;

        static bool TailCondition(WhiteSpaceItem item) => item.Type is IComment;

        internal static(CommentGroup[], LinesAndSpaces) Create
            (IEnumerable<WhiteSpaceItem> items, IConfiguration configuration)
        {
            var groups = items.SplitAndTail(TailCondition);
            var commentGroups = groups
                .Items
                .Select(items => new CommentGroup(items, configuration))
                .ToArray();

            var linesAndSpaces =
                    groups.Tail.Any()
                        ? LinesAndSpaces.Create(groups.Tail, configuration)
                        : LinesAndSpaces.Create(items.Last().SourcePart.End, configuration)
                ;

            return (commentGroups, linesAndSpaces);
        }

        internal static(CommentGroup[] Comments, LinesAndSpaces LinesAndSpaces) Create
            (SourcePosition anchor, WhiteSpaceView.IConfiguration configuration)
            => (new CommentGroup[0], LinesAndSpaces.Create(anchor, configuration));

        internal IEnumerable<Edit> GetEdits(bool isSeparatorRequired, int indent)
        {
            if(indent > 0)
            {
                NotImplementedMethod(isSeparatorRequired, indent);
                return default;
            }

            return LinesAndSpaces.GetEdits(isSeparatorRequired, indent);
        }
    }
}