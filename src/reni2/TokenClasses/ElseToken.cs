using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, IValueProvider
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            Tracer.Assert(syntax.Left != null);
            Tracer.Assert(syntax.Left.TokenClass is ThenToken);
            return CondSyntax.Create(syntax.Left.Left, syntax.Left.Right, syntax.Right, syntax);
        }
    }
}