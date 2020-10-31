using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
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
            Items = items;
            Items.Any().Assert();
            Main.AssertIsNotNull();
        }

        internal SourcePart[] SourceParts => Items.Select(item => item.Token.SourcePart()).ToArray();
        internal SourcePart SourcePart => SourceParts.Combine();

        public IEnumerable<Issue> Issues => Items.SelectMany(node => node.Issues);

        public bool IsEmpty => !Items.Any();

        internal BinaryTree Main => this.CachedValue(GetMain);
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        internal Anchor GetLeftOf(BinaryTree target) => GetLeftOf(target.Token.SourcePart().Start);
        internal Anchor GetRightOf(BinaryTree target) => GetRightOf(target.Token.SourcePart().End);

        [PublicAPI]
        internal Anchor GetLeftOf(SourcePosition position)
            => new Anchor(Items.Where(item => item.Token.SourcePart() < position).ToArray());

        [PublicAPI]
        internal Anchor GetRightOf(SourcePosition position)
            => new Anchor(Items.Where(item => position < item.Token.SourcePart()).ToArray());

        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        internal static Anchor Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new Anchor(leftAnchor, rightAnchor);

        internal static Anchor Create(BinaryTree leftAnchor)
            => new Anchor(leftAnchor.AssertNotNull());

        public static Anchor Create(IEnumerable<BinaryTree> items)
            => new Anchor(items.ToArray());

        public Anchor Combine(Anchor other)
        {
            if(other == null || !other.Items.Any())
                return this;
            return new Anchor(other.Items.Concat(Items).ToArray());
        }

        BinaryTree GetMain()
        {
            var binaryTrees = Items
                .Where(node => !(node.TokenClass is IErrorToken)).ToArray();
            var enumerable = binaryTrees
                .Where(node => Items.All(parent => !node.HasAsParent(parent))).ToArray();
            return enumerable.Single();
        }
    }
}