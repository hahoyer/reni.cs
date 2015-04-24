using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Struct
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AtToken : InfixSyntaxToken
    {
        public const string TokenId = "_A_T_";
        public override string Id => TokenId;
        public override Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right)
            => left.AtTokenResult(callContext, category, right);
    }
}