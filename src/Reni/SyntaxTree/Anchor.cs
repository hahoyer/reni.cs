using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    public sealed class Anchor : DumpableObject, ValueCache.IContainer
    {
        public readonly BinaryTree[] Items;

        Anchor(params BinaryTree[] items)
        {
            Items = items
                .Where(item => item != null)
                .OrderBy(item => item.Token.Characters.Position).ToArray();
            Items.Any().Assert();
            //Main.AssertIsNotNull();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        [DisableDump]
        internal SourcePart[] SourceParts => Items.SourceParts();

        [DisableDump]
        internal SourcePart SourcePart => SourceParts.Combine();

        [DisableDump]
        public IEnumerable<Issue> Issues => Items.SelectMany(node => node.Issues);

        [DisableDump]
        public bool IsEmpty => !Items.Any();

        [DisableDump]
        internal BinaryTree Main => this.CachedValue(GetMain);

        internal Anchor GetLeftOf(BinaryTree target) => GetLeftOf(target.Token.SourcePart().Start);
        internal Anchor GetRightOf(BinaryTree target) => GetRightOf(target.Token.SourcePart().End);

        [PublicAPI]
        internal Anchor GetLeftOf(SourcePosition position)
            => new(Items.Where(item => item.Token.SourcePart() < position).ToArray());

        [PublicAPI]
        internal Anchor GetRightOf(SourcePosition position)
            => new(Items.Where(item => position < item.Token.SourcePart()).ToArray());

        internal static Anchor Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new(leftAnchor, rightAnchor);

        internal static Anchor Create(BinaryTree leftAnchor)
            => new(leftAnchor.AssertNotNull());

        internal static Anchor Create(params BinaryTree[] items) => new(items);

        internal Anchor Combine(Anchor other) => Combine(other?.Items);

        internal Anchor Combine(BinaryTree[] other)
        {
            if(other == null || !other.Any())
                return this;
            return new Anchor(other.Concat(Items).ToArray());
        }

        BinaryTree GetMain()
        {
            var binaryTrees = Items
                .Where(node => !(node.TokenClass is IErrorToken)).ToArray();
            return binaryTrees
                .SingleOrDefault(node => Items.All(parent => !node.HasAsParent(parent)));
        }

        public void SetSyntax(Syntax syntax)
        {
            foreach(var item in Items)
                item.SetSyntax(syntax);
        }

        public static Anchor CreateAll(BinaryTree target)
            => Create(target.GetNodesFromLeftToRight().ToArray());
    }
}