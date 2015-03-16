using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.ReniSyntax
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList(IToken token)
            : base(token)
        {
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}