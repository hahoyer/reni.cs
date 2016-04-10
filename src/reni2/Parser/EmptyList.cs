using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.Parser
{
    sealed class EmptyList : Value
    {
        internal override SourcePart Token { get; }

        public EmptyList(SourcePart token)
        {
            Token = token;
            StopByObjectIds();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}