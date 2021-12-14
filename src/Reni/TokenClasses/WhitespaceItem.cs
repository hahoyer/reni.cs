using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.TokenClasses.Whitespace;

namespace Reni.TokenClasses
{
    sealed class WhitespaceItem : DumpableObject, IWhitespaceItem
    {
        internal readonly SourcePart SourcePart;

        internal readonly IItemType Type;

        [DisableDump]
        [PublicAPI]
        internal readonly IParent Parent;

        WhitespaceItem[] ItemsCache;

        internal WhitespaceItem(SourcePart sourcePart)
            : this(RootType.Instance, sourcePart, null) { }

        internal WhitespaceItem(IItemType type, SourcePart sourcePart, IParent parent)
        {
            Type = type;
            SourcePart = sourcePart;
            Parent = parent;
        }

        IWhitespaceItem IParent.GetItem<TItemType>()
            => Type is TItemType? this : Parent?.GetItem<TItemType>();

        IParent IParent.Parent => Parent;
        IItemType IParent.Type => Type;

        SourcePart IWhitespaceItem.SourcePart => SourcePart;

        IItemType IWhitespaceItem.Type => Type;

        protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

        [DisableDump]
        internal int TargetLineBreakCount => Items.Sum(item => item.TargetLineBreakCount);

        [EnableDump]
        internal WhitespaceItem[] Items => ItemsCache ??= GetItems();

        public bool? GetSeparatorRequest(bool areEmptyLinesPossible)
        {
            if(SourcePart.Position == 0 || SourcePart.End.IsEnd)
                return false;

            if(SourcePart.Length == 0)
                return null;

            var item = GetSeparatorRelevantItem(areEmptyLinesPossible);
            if(item != null)
                return item.Type is IComment;
            return null;
        }

        WhitespaceItem GetSeparatorRelevantItem(bool areEmptyLinesPossible)
        {
            if(Type is IComment or IStableLineBreak ||
               areEmptyLinesPossible && Type is IVolatileLineBreak)
                return this;
            return Items
                .Select(item => item.GetSeparatorRelevantItem(areEmptyLinesPossible))
                .FirstOrDefault(item => item != null);
        }

        WhitespaceItem[] GetItems()
            => (Type as IItemsType)?.GetItems(SourcePart, this).ToArray() ?? new WhitespaceItem[0];

        internal WhitespaceItem LocateItem(SourcePosition offset)
        {
            (SourcePart.Start <= offset).Assert();

            if(SourcePart.End <= offset)
                return null;

            if(!Items.Any())
                return this;

            return Items
                .Select(item1 => item1.LocateItem(offset))
                .FirstOrDefault(item => item != null);
        }

        internal string FlatFormat(bool areEmptyLinesPossible)
        {
            if(SourcePart.Length == 0)
                return "";

            if(Items.Any())
            {
                var results = Items.Select(item => item.FlatFormat(areEmptyLinesPossible));
                return results.Any(result => result == null)? null : results.Stringify("");
            }

            if(Type is IStableLineBreak || areEmptyLinesPossible && Type is IVolatileLineBreak)
                return null;

            return Type is ILineBreak? "" : SourcePart.Id;
        }
    }
}