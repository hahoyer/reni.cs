using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;

namespace Reni.ReniSyntax
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList(SourcePart token)
            : base(token)
        {}

        protected override string GetNodeDump() { return "()"; }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return context.RootContext.VoidResult(category);
        }

        internal sealed class Half : CompileSyntax
        {
            public Half(SourcePart leftToken)
                : base(leftToken)
            {}

            protected override string GetNodeDump() { return "("; }
            internal override Syntax RightParenthesis(int level, SourcePart token) { return new EmptyList(Token); }
        }
    }
}