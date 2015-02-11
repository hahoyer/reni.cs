using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => "\0}])".Substring(level, 1);
        readonly int _level;

        public RightParenthesis(int level) { _level = level; }

        string ITokenClassWithId.Id => Id(_level);
        protected override Syntax Suffix(Syntax left, SourcePart token) => left.RightParenthesis(_level, token);
    }

    sealed class EndToken : TokenClass
    {
        protected override Syntax Suffix(Syntax left, SourcePart token) => left.ToCompiledSyntax;
    }
}