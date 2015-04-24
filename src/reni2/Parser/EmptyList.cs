using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;

namespace Reni.Parser
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList() { StopByObjectIds(); }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}