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
    sealed class AtToken : InfixPrefixSyntaxToken
    {
        public const string TokenId = "_A_T_";
        public override string Id => TokenId;

        protected override Result Result
            (ContextBase context, Category category, Value left, Value right)
        {
            var target = context.ResultAsReference(category.Typed, left);
            return target
                .Type
                .FindRecentCompoundView
                .AccessViaPositionExpression(category, right.Result(context))
                .ReplaceArg(target);
        }

        protected override Result Result(ContextBase context, Category category, Value right)
            => context.FindRecentCompoundView.AtTokenResult(category, right.Result(context));
    }
}