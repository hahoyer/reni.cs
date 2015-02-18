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
        public EmptyList(SourcePart all, SourcePart token)
            : base(all, token) {}

        protected override string GetNodeDump() => "()";
        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new EmptyList(SourcePart + sourcePart, Token);
    }
}