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

        readonly int _level;

        public List(int level) { _level = level; }

        public override string Id => TokenId(_level);
        [DisableDump]
        protected override ITreeItemFactory TreeItemFactory => ListTree.FactoryInstance;

        protected override Checked<Syntax> Terminal(SourcePart token)
            => ListSyntax(new EmptyList(), new EmptyList());

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => ListSyntax(new EmptyList(), right);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => ListSyntax(left, new EmptyList());

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => ListSyntax(left, right);

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher == this;

        ListSyntax ListSyntax(Syntax left, Syntax right)
            => new ListSyntax(this, left.ToList(this).Concat(right.ToList(this)).ToArray());
    }
}