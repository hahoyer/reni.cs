using System.Linq;
using System.Collections.Generic;
using System;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.TokenClasses
{
    sealed class NewValueToken : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, SourcePart token)
        {
            return context
                .FindRecentFunctionContextObject
                .CreateValueReferenceResult(category);
        }
    }
}