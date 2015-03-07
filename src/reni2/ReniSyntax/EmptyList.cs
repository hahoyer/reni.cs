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
        public EmptyList(SourcePart token, SourcePart all = null)
            : base(token, all) { }

        protected override string GetNodeDump() => "()";
        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new EmptyList(Token, SourcePart + sourcePart);
    }
}