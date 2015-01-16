using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.ReniSyntax
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList(SourcePart token)
            : base(token) { }

        protected override string GetNodeDump() => "()";
        internal override Result ObtainResult(ContextBase context, Category category) => context.RootContext.VoidType.Result(category);
    }
}