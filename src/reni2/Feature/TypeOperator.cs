using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Feature
{
    sealed class TypeOperator : Suffix
    {
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            if(category.HasType)
                return left
                    .Type(context)
                    .TypeForTypeOperator
                    .UniqueTypeType
                    .Result(category);
            return context
                .RootContext
                .VoidResult(category);
        }
    }
}