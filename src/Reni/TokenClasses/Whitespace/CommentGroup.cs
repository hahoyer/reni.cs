using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace
{
    class CommentGroup : DumpableObject
    {
        internal interface IConfiguration : LinesAndSpaces .IConfiguration { }
        
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
            LinesAndSpaces = LinesAndSpaces.Create(items, configuration);
        }

        [DisableDump]
        internal bool IsSeparatorRequired => Main.Type.IsSeparatorRequired;

        static bool TailCondition(WhiteSpaceItem item) => item.Type is IComment;

        internal static(CommentGroup[], LinesAndSpaces) Create
            (IEnumerable<WhiteSpaceItem> items, IConfiguration configuration)
        {
            var groups = items.SplitAndTail(TailCondition);
            return (groups.Items.Select(items => new CommentGroup(items, configuration)).ToArray()
                , LinesAndSpaces.Create(groups.Tail, configuration));
        }

        internal IEnumerable<Edit> GetEdits(bool isSeparatorRequired, int indent)
        {
            if(indent > 0 ) 
            {
                NotImplementedMethod(isSeparatorRequired, indent);
                return default;
            }

            return LinesAndSpaces.GetEdits(isSeparatorRequired, indent);
        }
    }
}