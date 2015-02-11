using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class FunctionInstanceToken : SuffixToken, ITokenClassWithId
    {
        public const string Id = "function_instance";
        string ITokenClassWithId.Id => Id;
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
            => left
                .Result(context, category.Typed)
                .Type
                .FunctionInstance
                .Result(category, left.Result(context, category.Typed));
    }
}