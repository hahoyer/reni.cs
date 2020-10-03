using System.Collections.Generic;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class EmptyList : Syntax
    {
        public EmptyList(BinaryTree binaryTree)
            : base(binaryTree) { StopByObjectIds(); }

        protected override string GetNodeDump() => "()";

        protected override IEnumerable<Syntax> GetChildren() => new Syntax[0];

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}