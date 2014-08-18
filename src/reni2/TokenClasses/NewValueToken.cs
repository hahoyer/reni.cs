using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using Reni.Basics;
using Reni.Context;

namespace Reni.TokenClasses
{
    sealed class NewValueToken : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);
        }
    }
}