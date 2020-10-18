using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Helper
{
    abstract class BinaryTreeSyntaxWithParent<TResult> : TreeWithParentExtended<TResult, BinaryTree>
        where TResult : BinaryTreeSyntaxWithParent<TResult>
    {
        class CacheContainer
        {
            public ValueCache<Syntax> FlatSyntax;
        }

        readonly CacheContainer Cache = new CacheContainer();

        protected BinaryTreeSyntaxWithParent(BinaryTree flatItem, TResult parent, Func<Syntax> getFlatSyntax)
            : base(flatItem, parent)
        {
            Cache.FlatSyntax =
                ValueCacheExtension.NewValueCache(() => getFlatSyntax != null? getFlatSyntax() : GetFlatSyntax());
        }


        internal string FlatSyntaxDump => Cache.FlatSyntax.IsValid? FlatSyntax.Dump() : "<unknown>";

        [EnableDump]
        [EnableDumpExcept(null)]
        public TResult Left => ((ITree<TResult>)this).GetDirectChild(0);

        [EnableDump]
        [EnableDumpExcept(null)]
        public TResult Right => ((ITree<TResult>)this).GetDirectChild(1);

        [EnableDump]
        internal ITokenClass TokenClass => FlatItem.TokenClass;

        [DisableDump]
        internal Syntax FlatSyntax => Cache.FlatSyntax.Value;

        internal TResult GetRightNeighbor(int current)
            => RightNeighbor
                .Chain(node => node.RightNeighbor)
                .Top(node => node.FlatItem.Token.Characters.EndPosition > current)
                .AssertNotNull();

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);

        internal bool Contains(int current)
            => FlatItem.SourcePart.Position <= current && current < FlatItem.SourcePart.EndPosition;
              

        Syntax GetFlatSyntax()
        {
            var result = Parent
                .FlatSyntax
                .GetNodesFromTopToBottom(node => node?.Anchor != null)
                .SingleOrDefault(node => node.Anchor == FlatItem);

            if(result != null)
                return result;

            if(TokenClass is ILeftBracket && TokenClass.IsBelongingTo(Parent.TokenClass))
                return Parent.FlatSyntax;

            if(TokenClass is List)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;

                NotImplementedMethod();
                return default;
            }

            if(TokenClass is Colon)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;
                NotImplementedMethod();
                return default;
            }

            NotImplementedMethod();
            return default;
        }
    }
}