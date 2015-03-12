using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class NewValueToken : NonSuffix
    {
        public const string TokenId = "new_value";
        public override string Id => TokenId;
        public override Result Result(ContextBase context, Category category, IToken token)
            => context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);
        public override Result Result
            (ContextBase context, Category category, IToken token, CompileSyntax right)
        {
            NotImplementedMethod(context, category, token, right);
            return null;
        }
    }
}