using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class ArgToken : NonSuffix
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .ArgReferenceResult(category);
        }

        public override Result Result(ContextBase context, Category category, TokenData token, CompileSyntax right)
        {
            return context
                .FunctionalArgResult(category, right);
        }
    }
}