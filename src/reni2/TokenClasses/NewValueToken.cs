using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class NewValueToken : NonSuffixSyntaxToken
    {
        public const string TokenId = "new_value";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category)
            => context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);

        protected override Result Result
            (ContextBase context, Category category, Value right, Syntax token)
        {
            NotImplementedMethod(context, category, token, right);
            return null;
        }
    }
}