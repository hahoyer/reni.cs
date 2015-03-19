using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;

namespace Reni.ReniSyntax
{
    sealed class EmptyList : CompileSyntax
    {
        public EmptyList(SourcePosn position)
            : base(position.Token())
        {
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}