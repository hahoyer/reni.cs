using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    sealed class List : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => ",;.".Substring(level, 1);
        readonly int _level;
        public List(int level) { _level = level; }

        string ITokenClassWithId.Id => Id(_level);

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => ListSyntax(new EmptyList(token, token), token, right);

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => ListSyntax(left, token, new EmptyList(token, token));

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => ListSyntax(left, token, right);

        ListSyntax ListSyntax(Syntax left, SourcePart token, Syntax right)
            => new ListSyntax(this, token, left.ToList(this).Concat(right.ToList(this)).ToArray());
    }
}