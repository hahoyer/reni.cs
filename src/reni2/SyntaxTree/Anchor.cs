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
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        internal BinaryTree[] Items { get; private set; }

        Anchor() { }

        internal SourcePart[] SourceParts => Items.Select(item => item.Token.SourcePart()).ToArray();
        internal SourcePart SourcePart => SourceParts.Combine();

        public IEnumerable<Issue> Issues => Items.SelectMany(node => node.Issues);

        public bool IsEmpty => !Items.Any();

        internal Anchor GetLeftOf(BinaryTree target) => GetLeftOf(target.Token.SourcePart().Start);
        internal Anchor GetRightOf(BinaryTree target) => GetRightOf(target.Token.SourcePart().End);

        [PublicAPI]
        internal Anchor GetLeftOf(SourcePosition position)
            => new Anchor {Items = Items.Where(item => item.Token.SourcePart() < position).ToArray()};

        [PublicAPI]
        internal Anchor GetRightOf(SourcePosition position)
            => new Anchor {Items = Items.Where(item => position < item.Token.SourcePart()).ToArray()};

        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        internal static Anchor Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new Anchor
            {
                Items = T(leftAnchor, rightAnchor)
            };

        internal static Anchor Create(BinaryTree leftAnchor)
            => new Anchor
            {
                Items = T(leftAnchor.AssertNotNull())
            };

        public static Anchor Create(IEnumerable<BinaryTree> left)
            => new Anchor
            {
                Items = left.ToArray()
            };

        public Anchor Combine(Anchor other)
        {
            if(other == null || !other.Items.Any())
                return this;

            return new Anchor
            {
                Items = other.Items.Concat(Items).ToArray()
            };
        }

        public BinaryTree GetMain() => Items
            .Single(node => Items.All(parent => node.Parent != parent));

        internal BinaryTree Main => this.CachedValue(GetMain);
    }
}