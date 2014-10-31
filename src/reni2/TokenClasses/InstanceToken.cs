using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class InstanceToken : InfixToken, IPendingProvider
    {
        public override Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return left
                .Type(context)
                .InstanceResult(category, c => context.ResultAsReference(c, right));
        }

        Result IPendingProvider.ObtainResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            if(category <= Category.Type.Replenished)
                return Result(context, category, left, right);
            NotImplementedMethod(context, category, left, right);
            return null;
        }
    }
}