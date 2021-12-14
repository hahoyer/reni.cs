using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;
using Reni.TokenClasses.Whitespace;

namespace ReniUI.Formatting
{
    class SpacesGroup : DumpableObject
    {
        internal readonly WhitespaceItem[] Items;
        internal readonly WhitespaceItem Tail;

        public SpacesGroup(IEnumerable<WhitespaceItem> allItems)
        {
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out Items);
            groups.TryGetValue(true, out var tails);

            Tail = tails?.Single();
            Items ??= new WhitespaceItem[0];
        }

        internal static SpacesGroup[] Create(WhitespaceItem[] items)
            => items != null
                ? items.Split(TailCondition, false)
                    .Select(items => new SpacesGroup(items))
                    .ToArray()
                : new SpacesGroup[0];

        static bool TailCondition(WhitespaceItem item) => item.Type is not ISpace;

        internal IEnumerable<Edit> Get(IEditPiecesConfiguration parameter, bool isSeparatorRequired)
        {
            if(Tail == null)
            {
                if(isSeparatorRequired)
                {
                    if(Items.Length == 0)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        NotImplementedMethod(parameter, isSeparatorRequired);
                        return default;
                    }

                    if(Items.Length == 1)
                        return new Edit[0];

                    var sourcePart = Items[1].SourcePart.Start.Span(Items.Length - 1);
                    return new[] { new Edit(sourcePart, "", "-Spaces") };
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                NotImplementedMethod(parameter, isSeparatorRequired);
                return default;
            }

            NotImplementedMethod(parameter, isSeparatorRequired);
            return default;
        }
    }

    class LineGroup : DumpableObject
    {
        internal interface IConfiguration
        {
            int MinimalLineBreakCount { get; }
            int? EmptyLineLimit { get; }
        }

        readonly IConfiguration Configuration;

        internal readonly SpacesGroup[] Items;
        internal readonly WhitespaceItem Tail;

        LineGroup(IEnumerable<WhitespaceItem> allItems, IConfiguration configuration)
        {
            Configuration = configuration;
            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out var items);
            groups.TryGetValue(true, out var tails);

            Tail = tails?.Single();

            Items = SpacesGroup.Create(items);
        }

        internal static LineGroup[] Create(WhitespaceItem[] items, IConfiguration configuration)
            => items == null
                ? new LineGroup[0]
                : items.Split(TailCondition, false)
                    .Select(items => new LineGroup(items, configuration))
                    .ToArray();

        static bool TailCondition(WhitespaceItem item) => item.Type is IVolatileLineBreak;

        internal IEnumerable<Edit> Get(IEditPiecesConfiguration parameter, bool isSeparatorRequired)
        {
            if(Tail == null)
            {
                if(Items.Length == 1)
                {
                    var item = Items[0];
                    return item.Get(parameter, isSeparatorRequired);
                }

                NotImplementedMethod(parameter, isSeparatorRequired);
                return default;
            }

            if(Configuration.EmptyLineLimit == null)
                return new Edit[0];

            NotImplementedMethod(parameter, isSeparatorRequired);
            return default;
        }
    }
}