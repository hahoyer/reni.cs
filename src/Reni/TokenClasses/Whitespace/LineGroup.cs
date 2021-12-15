using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace
{
    class LineGroup : DumpableObject
    {
        internal interface IConfiguration
        { }

        [EnableDump]
        readonly SpacesGroup[] Spaces;

        [EnableDump]
        readonly WhiteSpaceItem Main;

        readonly IConfiguration Configuration;

        LineGroup(IEnumerable<WhiteSpaceItem> allItems, IConfiguration configuration)
        {
            Configuration = configuration;
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out var items);
            groups.TryGetValue(true, out var tails);

            Main = tails?.Single();

            Spaces = SpacesGroup.Create(items);
        }

        internal static(LineGroup[], SpacesGroup[]) Create(WhiteSpaceItem[] items, IConfiguration configuration)
        {
            var groups = items.SplitAndTail(TailCondition);
            return (groups.Items.Select(items => new LineGroup(items, configuration)).ToArray()
                , SpacesGroup.Create(groups.Tail));
        }

        static bool TailCondition(WhiteSpaceItem item) => item.Type is IVolatileLineBreak;

        internal IEnumerable<Edit> GetEdits()
        {
            if(Spaces.Any())
            {
                NotImplementedMethod();
                return default;
            }

            return new Edit[0];
        }
    }
}