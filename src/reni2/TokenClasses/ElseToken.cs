using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, ITokenClassWithId
    {
        public const string Id = "else";
        string ITokenClassWithId.Id => Id;
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateElseSyntax(token, right.ToCompiledSyntax);
    }
}