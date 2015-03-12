using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class InstanceToken : InfixToken, IPendingProvider
    {
        public const string TokenId = "instance";
        public override string Id => TokenId;
        public override Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => left
                .Type(context)
                .InstanceResult(category, c => context.ResultAsReference(c, right));

        Result IPendingProvider.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            if(category <= Category.Type.Replenished)
                return Result(context, category, left, right);
            NotImplementedMethod(context, category, left, right);
            return null;
        }
    }
}