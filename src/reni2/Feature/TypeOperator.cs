using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TypeOperator : SuffixToken, ITokenClassWithId
    {
        public const string Id = "type";
        string ITokenClassWithId.Id => Id;
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
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