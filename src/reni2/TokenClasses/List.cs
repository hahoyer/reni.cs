using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass
    {
        public static string TokenId(int level) => ",;.".Substring(level, 1);
        readonly int _level;
        public List(int level) { _level = level; }

        public override string Id => TokenId(_level);

        protected override Syntax Terminal(IToken token)
            =>
                ListSyntax
                    (
                        new EmptyList(),
                        token,
                        new EmptyList());

        protected override Syntax Prefix(IToken token, Syntax right)
            => ListSyntax(new EmptyList(), token, right);

        protected override Syntax Suffix(Syntax left, IToken token)
            => ListSyntax(left, token, new EmptyList());

        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
            => ListSyntax(left, token, right);

        ListSyntax ListSyntax(Syntax left, IToken token, Syntax right)
            => new ListSyntax(this, left.ToList(this).Concat(right.ToList(this)).ToArray());
    }
}