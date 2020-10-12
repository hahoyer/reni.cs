using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;

namespace ReniUI.Helper
{
    class BinaryTree : BinaryTreeSyntaxWithParent<BinaryTree>
    {
        class CacheContainer
        {
            public FunctionCache<int, BinaryTree> ExtendedLocateByPosition;
            public FunctionCache<int, BinaryTree> LocateByPosition;
        }

        [DisableDump]
        internal readonly Syntax Syntax;

        readonly CacheContainer Cache = new CacheContainer();

        internal BinaryTree(Reni.TokenClasses.BinaryTree flatItem, Syntax syntax)
            : this(flatItem, null, syntax) { }

        BinaryTree(Reni.TokenClasses.BinaryTree flatItem, BinaryTree parent, Syntax syntax)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, BinaryTree>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, BinaryTree>(ExtendedLocateByPositionForCache);

            Syntax = FindAndLink(syntax);
        }

        [DisableDump]
        internal IEnumerable<BinaryTree> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        Syntax FindAndLink(Syntax syntax)
        {
            syntax ??= Find();
            syntax.Binary = this;
            return syntax;
        }


        Syntax Find()
        {
            var result = Parent
                .Syntax
                .DirectChildren
                .FirstOrDefault(node=> node?.FlatItem.Binary == FlatItem);
            if(result != null)
                return result;

            nameof(FlatItem).IsSetTo(FlatItem).FlaggedLine();

            var xx = Parent
                .Syntax
                .DirectChildren
                .Where(node => node?.FlatItem.Binary != null)
                .ToArray();

            Parent
                .Syntax
                .DirectChildren
                .Where(node => node?.FlatItem.Binary != null)
                .Select(node=> node.FlatItem.Binary.Dump())
                .Stringify("\n")
                .Indent()
                .Log();

            Parent.Syntax.Dump().Log();


            NotImplementedMethod();
            var syntax = new Syntax(new LinkSyntax(FlatItem));
            return syntax;
        }

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id})";

        public BinaryTree LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override BinaryTree Create
            (Reni.TokenClasses.BinaryTree flatItem) => new BinaryTree(flatItem, this, null);

        internal BinaryTree Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        BinaryTree CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        BinaryTree LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        BinaryTree LocateByPositionForCache(int current)
            => Left?.LocateByPositionOrDefault(current) ??
               Right?.LocateByPositionOrDefault(current) ??
               this;

        BinaryTree ExtendedLocateByPositionForCache(int current)
            => Contains(current)
                ? Left?.ExtendedLocateByPositionOrDefault(current) ??
                  Right?.ExtendedLocateByPositionOrDefault(current) ??
                  this
                : Parent.ExtendedLocateByPositionForCache(current);

        BinaryTree LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        BinaryTree ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;

        internal BinaryTree Find(Reni.TokenClasses.BinaryTree target)
        {
            if(FlatItem == target)
                return this;

            var targetSourcePart = target.SourcePart;
            Tracer.Assert(FlatItem.SourcePart.Contains(targetSourcePart));
            return targetSourcePart.EndPosition <= Token.Characters.Position
                ? Left.Find(target)
                : Right.Find(target);
        }
    }

    class LinkSyntax : Reni.Parser.Syntax.NoChildren
    {
        public LinkSyntax(Reni.TokenClasses.BinaryTree binary)
            : base(binary) { }
    }
}