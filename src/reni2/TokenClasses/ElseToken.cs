using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, IValueProvider
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(left.TokenClass is ThenToken);
            return CondSyntax.Create(left.Left, left.Token.Characters, left.Right, right);
        }
    }
}