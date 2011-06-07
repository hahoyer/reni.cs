using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class ThisToken : Terminal
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentStructContext
                .ThisReferenceResult(category);
        }
    }
}