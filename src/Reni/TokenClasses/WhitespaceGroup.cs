using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class WhitespaceGroup : DumpableObject, WhitespaceGroup.IParent
    {
        internal interface IParent { }

        internal interface IComment { }

        internal interface ISpace { }

        internal interface ILineEnd { }

        internal interface IItemType
        {
        }

        sealed class ItemPrototype : DumpableObject
        {
            internal readonly IItemType Type;
            internal readonly IMatch Match;

            public ItemPrototype(IItemType type, IMatch match)
            {
                Type = type;
                Match = match;
            }
        }

        abstract class ItemType : DumpableObject, IItemType
        {
            internal virtual ItemPrototype[] ItemPrototypes => null;
        }

        sealed class RootType : ItemType
        {
            internal static readonly RootType Instance = new();

            internal override ItemPrototype[] ItemPrototypes { get; } =
            {
                new(SpaceType.Instance, Lexer.Instance.Space)
                , new(LineEndType.Instance, Lexer.Instance.LineEnd)
                , new(LineCommentType.Instance, Lexer.Instance.LineComment)
                , new(InlineCommentType.Instance, Lexer.Instance.InlineComment),
            };
        }

        sealed class SpaceType : ItemType, ISpace
        {
            internal static readonly SpaceType Instance = new();
        }

        sealed class LineEndType : ItemType, ILineEnd
        {
            internal static readonly LineEndType Instance = new();
        }

        sealed class LineCommentType : ItemType, IComment
        {
            internal static readonly LineCommentType Instance = new();
        }

        sealed class InlineCommentType : ItemType, IComment
        {
            internal static readonly InlineCommentType Instance = new();
        }

        internal readonly SourcePart SourcePart;

        internal readonly IItemType Type;

        readonly IParent Parent;

        WhitespaceGroup[] ItemsCache;

        internal WhitespaceGroup(SourcePart sourcePart)
            : this(RootType.Instance, sourcePart, null) { }

        WhitespaceGroup(IItemType type, SourcePart sourcePart, IParent parent)
        {
            Type = type;
            SourcePart = sourcePart;
            Parent = parent;
        }

        protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

        [DisableDump]
        internal int TargetLineBreakCount => Items.Sum(item => item.TargetLineBreakCount);

        WhitespaceGroup[] Items => ItemsCache ??= GetItems().ToArray();

        IEnumerable<WhitespaceGroup> GetItems()
        {
            var itemPrototypes = ((ItemType)Type).ItemPrototypes;
            if(itemPrototypes == null)
                yield break;

            var sourcePosition = SourcePart.Start.Clone;
            while(sourcePosition < SourcePart.End)
            {
                var result = itemPrototypes
                    .Select(p => (p.Type, Length: sourcePosition.Match(p.Match)))
                    .FirstOrDefault(p => p.Length != null);

                result.AssertIsNotNull();
                result.Length.AssertIsNotNull();
                yield return new(result.Type, sourcePosition.Span(result.Length.Value), null);
                sourcePosition += result.Length.Value;
            }
        }

        internal WhitespaceGroup LocateItem(SourcePosition offset)
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
    }
}