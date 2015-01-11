using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.Feature
{
    sealed class TypeOperator : SuffixToken
    {
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            if(category.HasType)
                return left
                    .Type(context)
                    .TypeForTypeOperator
                    .TypeType
                    .Result(category);
            return context
                .RootContext
                .VoidResult(category);
        }
    }
}