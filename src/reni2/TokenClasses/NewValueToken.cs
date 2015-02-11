using System.Linq;
using System.Collections.Generic;
using System;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class NewValueToken : TerminalToken, ITokenClassWithId
    {
        public const string Id = "new_value";
        string ITokenClassWithId.Id => Id;
        public override Result Result(ContextBase context, Category category, SourcePart token)
            => context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);
    }
}