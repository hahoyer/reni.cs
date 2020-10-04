﻿using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TypeOperator : SuffixSyntaxToken
    {
        public const string TokenId = "type";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, Parser.ValueSyntax left)
        {
            if(category.HasType)
            {
                var leftType = left.Type(context);
                if(leftType.HasIssues)
                    return new Result(category, leftType.Issues);

                return leftType
                    .TypeForTypeOperator
                    .TypeType
                    .Result(category);
            }

            return context
                .RootContext.VoidType.Result(category);
        }
    }
}