using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class FunctionInstanceToken : SuffixSyntaxToken
    {
        public const string TokenId = "function_instance";
        public override string Id => TokenId;
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
            => left
                .Result(context, category.Typed)
                .Type
                .FunctionInstance
                .Result(category, left.Result(context, category.Typed));
    }
}