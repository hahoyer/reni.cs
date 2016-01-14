using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Formatting;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class InstanceToken : InfixSyntaxToken, IPendingProvider, IChainLink, IRecursionHandler
    {
        public const string TokenId = "instance";
        public override string Id => TokenId;

        protected override Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => left
                .Type(context)
                .InstanceResult(category, c => context.ResultAsReference(c, right));

        Result IPendingProvider.Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            if(category <= Category.Type.Replenished)
                return Result(context, category, left, right);
            NotImplementedMethod(context, category, left, right);
            return null;
        }

        Result IRecursionHandler.Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            CompileSyntax syntax,
            bool asReference)
        {
            if(!asReference && (category | pendingCategory) <= Category.Type)
                return syntax.ResultForCache(context, Category.Type);

            NotImplementedMethod(context, category, pendingCategory, syntax, asReference);
            return null;
        }
    }
}