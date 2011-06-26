using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class ArgToken : Terminal
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            var trace = token.ObjectId == -119 && category.HasRefs;
            StartMethodDump(trace, context, category, token);
            var result = context.CreateArgsReferenceResult(category);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}