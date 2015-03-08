using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;

namespace Reni.ReniSyntax
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList(Token token)
            : base(token) { }

        public EmptyList(EmptyList other, params ParsedSyntax[] parts)
            : base(other, parts) { }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);

        internal override CompileSyntax SurroundCompileSyntax(params ParsedSyntax[] parts)
            => new EmptyList(this, parts);
    }
}