using System.Linq;
using System.Collections.Generic;
using System;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class NewValueToken : NonSuffix, ITokenClassWithId
    {
        public const string Id = "new_value";
        string ITokenClassWithId.Id => Id;
        public override Result Result(ContextBase context, Category category, SourcePart token)
            => context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);
        public override Result Result
            (ContextBase context, Category category, SourcePart token, CompileSyntax right)
        {
            NotImplementedMethod(context, category, token, right);
            return null;
        }
    }
}