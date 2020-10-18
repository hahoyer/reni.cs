using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(BinaryTree anchor)
            : base(anchor)
            => StopByObjectIds();

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}