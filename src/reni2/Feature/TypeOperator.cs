using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Formatting;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TypeOperator : SuffixSyntaxToken, IChainLink
    {
        public const string TokenId = "type";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            if(category.HasType)
                return left
                    .Type(context)
                    .TypeForTypeOperator
                    .TypeType
                    .Result(category);
            return context
                .RootContext.VoidType.Result(category);
        }
    }
}