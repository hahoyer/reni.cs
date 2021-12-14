using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;
using Reni.TokenClasses.Whitespace;
using Reni.TokenClasses.Whitespace.Comment;

namespace ReniUI.Formatting
{
    class StableLineGroup : DumpableObject, LineGroup.IConfiguration
    {
        internal interface IConfiguration
        {
            int MinimalLineBreakCount { get; }
            int? EmptyLineLimit { get; }
        }

        readonly IConfiguration Configuration;

        [EnableDump]
        readonly LineGroup[] Items;

        [EnableDump]
        readonly WhitespaceItem Tail;


        StableLineGroup(IEnumerable<WhitespaceItem> allItems, IConfiguration configuration)
        {
            Configuration = configuration;

            var groups = allItems
                .GroupBy(TailCondition)
                .ToDictionary(item => item.Key, item => item.ToArray());

            groups.TryGetValue(false, out var items);
            groups.TryGetValue(true, out var tails);

            Tail = tails?.Single();
            Items = LineGroup.Create(items, this);
        }

        int? LineGroup.IConfiguration.EmptyLineLimit => Configuration.EmptyLineLimit;
        int LineGroup.IConfiguration.MinimalLineBreakCount => Configuration.MinimalLineBreakCount;

        [DisableDump]
        internal bool IsSeparatorRequired
        {
            get
            {
                if(Tail != null)
                    return Tail.Type.IsSeparatorRequired;


                NotImplementedMethod();
                return default;
            }
        }

        static bool TailCondition(WhitespaceItem item) => item.Type is IStable;

        internal IEnumerable<Edit> Get(IEditPiecesConfiguration parameter, bool isSeparatorRequired)
        {
            if(!Items.Any() &&
               (Tail?.Parent?.Type is IInline ||
                   Tail?.Parent?.Parent?.Type is IInline))
                return new Edit[0];

            if(Tail == null)
            {
                if(Items.Length == 1)
                    return Items[0].Get(parameter, isSeparatorRequired);

                NotImplementedMethod(parameter, isSeparatorRequired);
                return default;
            }

            NotImplementedMethod(parameter, isSeparatorRequired);
            return default;
        }

        public static IEnumerable<StableLineGroup> Create(WhitespaceItem[] items, WhiteSpaceView whiteSpaceView)
            => items
                .Split(TailCondition, false)
                .Select(items => new StableLineGroup(items, whiteSpaceView));
    }
}