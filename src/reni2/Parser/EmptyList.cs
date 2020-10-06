using System.Collections.Generic;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class EmptyList : ValueSyntax
    {
        public EmptyList(BinaryTree target)
            : base(target) { StopByObjectIds(); }

        protected override string GetNodeDump() => "()";

        protected override IEnumerable<Syntax> GetChildren() => new ValueSyntax[0];

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}