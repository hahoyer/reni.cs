using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.TokenClasses.Whitespace
{
    class SpacesGroup : DumpableObject
    {
        [EnableDump]
        readonly WhiteSpaceItem[] Items;
        [EnableDump]
        readonly WhiteSpaceItem Tail;

        SpacesGroup(IEnumerable<WhiteSpaceItem> allItems)
        {
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out Items);
            groups.TryGetValue(true, out var tails);

            Tail = tails?.Single();
            Items ??= new WhiteSpaceItem[0];
        }

        internal static SpacesGroup[] Create(WhiteSpaceItem[] items)
            => items != null
                ? items.Split(TailCondition, false)
                    .Select(items => new SpacesGroup(items))
                    .ToArray()
                : new SpacesGroup[0];

        static bool TailCondition(WhiteSpaceItem item) => item.Type is not ISpace;
    }
}