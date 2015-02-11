using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.Struct
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AtToken : InfixToken, ITokenClassWithId
    {
        public const string Id = "_A_T_";
        string ITokenClassWithId.Id => Id;
        public override Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right)
            => left.AtTokenResult(callContext, category, right);
    }
}