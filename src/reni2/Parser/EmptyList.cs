using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(BinaryTree anchor)
            : base(anchor)
            => StopByObjectIds(220);

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}