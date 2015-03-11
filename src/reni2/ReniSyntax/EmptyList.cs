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
        public EmptyList(SourcePosn posn)
            : base(new Token(posn.Span(0),null))
        {
            StopByObjectIds();
        }

        public EmptyList(EmptyList other, params ParsedSyntax[] parts)
            : base(other, parts)
        {
            StopByObjectIds(19,20);
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);

        internal override CompileSyntax SurroundCompileSyntax(params ParsedSyntax[] parts)
            => new EmptyList(this, parts);
    }
}