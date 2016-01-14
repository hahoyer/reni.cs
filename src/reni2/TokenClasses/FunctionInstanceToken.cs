using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class FunctionInstanceToken : SuffixSyntaxToken
    {
        public const string TokenId = "function_instance";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, CompileSyntax left)
            => context.Result(category.Typed, left)
                .Type
                .FunctionInstance
                .Result(category, context.Result(category.Typed, left));
    }
}