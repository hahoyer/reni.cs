using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Struct
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AtToken : InfixPrefixSyntaxToken
    {
        public const string TokenId = "_A_T_";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => left.AtTokenResult(context, category, right);

        protected override Result Result(ContextBase context, Category category, CompileSyntax right)
            => context.AtTokenResult(category, right);
    }
}