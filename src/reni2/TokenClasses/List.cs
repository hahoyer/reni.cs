using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Formatting;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level) => ",;.".Substring(level, 1);

        [DisableDump]
        internal readonly int Level;

        public List(int level) { Level = level; }

        public override string Id => TokenId(Level);

        protected override Checked<Syntax> Terminal(SourcePart token)
            =>
                ListSyntax
                    (
                        new EmptyList(),
                        new EmptyList());

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => ListSyntax(new EmptyList(), right);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => ListSyntax(left, new EmptyList());

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => ListSyntax(left, right);

        [DisableDump]
        protected override ITreeItemFactory TreeItemFactory => ListTree.FactoryInstance;

        ListSyntax ListSyntax(Syntax left, Syntax right)
            => new ListSyntax(this, left.ToList(this).Concat(right.ToList(this)).ToArray());

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;
    }
}