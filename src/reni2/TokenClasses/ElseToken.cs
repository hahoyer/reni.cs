using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        protected override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => left.CreateElseSyntax(right.ToCompiledSyntax);

    }
}