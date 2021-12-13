using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class WhitespaceGroup : DumpableObject, WhitespaceGroup.IParent
    {
        internal interface IParent { }

        internal interface IComment { }

        internal interface ISpace { }

        internal interface ILineBreak { }

        internal interface IItemType { }

        internal interface IStableLineBreak : ILineBreak { }

        interface IVolatileLineBreak : ILineBreak { }

        interface IItemsType
        {
            IEnumerable<WhitespaceGroup> GetItems(SourcePart sourcePart, IParent parent);
        }

        abstract class VariantListType : DumpableObject, IItemsType, IItemType
        {
            IEnumerable<WhitespaceGroup> IItemsType.GetItems(SourcePart sourcePart, IParent parent)
            {
                var sourcePosition = sourcePart.Start.Clone;
                while(sourcePosition < sourcePart.End)
                {
                    var valueTuples = VariantPrototypes
                        .Select(p => (p.Type, Length: sourcePosition.Span(sourcePart.End).Match(p.Match)))
                        .ToArray();
                    var result = valueTuples
                        .FirstOrDefault(p => p.Length != null);

                    result.AssertIsNotNull();
                    result.Length.AssertIsNotNull();
                    Tracer.ConditionalBreak(result.Length == 0);

                    var resultingSourcePart = sourcePosition.Span(T(result.Length.Value, sourcePart.EndPosition).Min());
                    (resultingSourcePart.End <= sourcePart.End).Assert();
                    yield return new(result.Type, resultingSourcePart, parent);
                    sourcePosition += result.Length.Value;
                }
            }

            [DisableDump]
            protected abstract ItemPrototype[] VariantPrototypes { get; }
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

        sealed class RootType : VariantListType
        {
            internal static readonly RootType Instance = new();

            [DisableDump]
            protected override ItemPrototype[] VariantPrototypes { get; } =
            {
                new(SpaceType.Instance, Lexer.Instance.Space)
                , new(LineEndType.Instance, Lexer.Instance.LineEnd)
                , new(LineCommentType.Instance, Lexer.Instance.LineComment)
                , new(InlineCommentType.Instance, Lexer.Instance.InlineComment),
            };
        }

        sealed class SpaceType : DumpableObject, ISpace, IItemType
        {
            internal static readonly SpaceType Instance = new();
        }

        sealed class LineEndType : DumpableObject, IVolatileLineBreak, IItemType
        {
            internal static readonly LineEndType Instance = new();
        }


        sealed class LineCommentType : DumpableObject, IItemType, IItemsType, IComment
        {
            sealed class HeadType : DumpableObject, IItemType
            {
                internal static readonly HeadType Instance = new();
            }

            sealed class TextLineType : DumpableObject, IItemType
            {
                internal static readonly TextLineType Instance = new();
            }

            sealed class TailType : DumpableObject, IItemType, IStableLineBreak
            {
                internal static readonly TailType Instance = new();
            }

            internal static readonly LineCommentType Instance = new();

            IEnumerable<WhitespaceGroup> IItemsType.GetItems(SourcePart sourcePart, IParent parent)
            {
                var headLength = sourcePart.Start.Match(Lexer.Instance.LineCommentHead);
                headLength.AssertIsNotNull();
                var head = sourcePart.Start.Span(headLength.Value);

                var tailLength = sourcePart.End.Match(Lexer.Instance.LineEnd, false);
                tailLength.AssertIsNotNull();
                var tail = sourcePart.End.Span(tailLength.Value);

                yield return new(HeadType.Instance, head, parent);
                yield return new(TextLineType.Instance, head.End.Span(tail.Start), parent);
                yield return new(TailType.Instance, tail, parent);
            }
        }

        sealed class InlineCommentType : DumpableObject, IComment, IItemsType, IItemType, IParent
        {
            sealed class HeadType : DumpableObject, IItemType
            {
                internal static readonly HeadType Instance = new();
            }

            sealed class IdType : DumpableObject, IItemType
            {
                internal static readonly IdType Instance = new();
            }

            sealed class TextType : VariantListType
            {
                sealed class TextLineType : DumpableObject, IItemType
                {
                    internal static readonly TextLineType Instance = new();
                }

                sealed class LineEndType : DumpableObject, IStableLineBreak, IItemType
                {
                    internal static readonly LineEndType Instance = new();
                }

                internal static readonly TextType Instance = new();

                [DisableDump]
                protected override ItemPrototype[] VariantPrototypes { get; } =
                {
                    new(LineEndType.Instance, Lexer.Instance.LineEnd)
                    , new(TextLineType.Instance, Lexer.Instance.LineEndOrEnd.Until)
                };
            }

            sealed class TailType : DumpableObject, IItemType
            {
                internal static readonly TailType Instance = new();
            }

            internal static readonly InlineCommentType Instance = new();

            IEnumerable<WhitespaceGroup> IItemsType.GetItems(SourcePart sourcePart, IParent parent)
            {
                var headLength = sourcePart.Start.Match(Lexer.Instance.InlineCommentHead);
                headLength.AssertIsNotNull();
                var head = sourcePart.Start.Span(headLength.Value);
                yield return new(HeadType.Instance, head, parent);

                var tailLength = sourcePart.Match(Lexer.Instance.InlineCommentTail, false);
                tailLength.AssertIsNotNull();
                var tail = sourcePart.End.Span(tailLength.Value);

                var contentWithNames = GetContentWithNames(head.End.Span(tail.Start));
                foreach(var item in contentWithNames)
                    yield return item;

                yield return new(TailType.Instance, tail, parent);
            }

            IEnumerable<WhitespaceGroup> GetContentWithNames(SourcePart sourcePart)
            {
                if(sourcePart.Length == 0)
                {
                    yield return new(TextType.Instance, sourcePart, this);
                    yield break;
                }

                var beforeWhiteSpace = sourcePart.Match(Match.WhiteSpace.Until);
                if(beforeWhiteSpace == null)
                {
                    yield return new(TextType.Instance, sourcePart, this);
                    yield break;
                }

                var open = sourcePart.Start.Span(beforeWhiteSpace.Value);

                var afterWhiteSpace = sourcePart.Match(Match.WhiteSpace.Until, false);
                afterWhiteSpace.AssertIsNotNull();
                var close = sourcePart.End.Span(afterWhiteSpace.Value);

                (open.Id == close.Id).Assert();
                if(open.Id.Length > 0)
                    yield return new(IdType.Instance, open, this);

                yield return new(TextType.Instance, open.End.Span(close.Start), this);

                if(close.Id.Length > 0)
                    yield return new(IdType.Instance, close, this);
            }
        }

        internal readonly SourcePart SourcePart;

        internal readonly IItemType Type;

        [DisableDump]
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

        [EnableDump]
        WhitespaceGroup[] Items => ItemsCache ??= GetItems();

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

        WhitespaceGroup GetSeparatorRelevantItem(bool areEmptyLinesPossible)
        {
            if(Type is IComment or IStableLineBreak || areEmptyLinesPossible && Type is IVolatileLineBreak)
                return this;
            return Items
                .Select(item => item.GetSeparatorRelevantItem(areEmptyLinesPossible))
                .FirstOrDefault(item => item != null);
        }

        WhitespaceGroup[] GetItems()
            => (Type as IItemsType)?.GetItems(SourcePart, this).ToArray() ?? new WhitespaceGroup[0];

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

            NotImplementedMethod(areEmptyLinesPossible);
            return default;
        }
    }
}