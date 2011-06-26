using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Struct
{
    internal sealed class AtToken : Infix
    {
        public override Result Result(ContextBase callContext, Category category, ICompileSyntax left, ICompileSyntax right)
        {
            return callContext
                .AtTokenResult(category, left, right);
        }
    }
}