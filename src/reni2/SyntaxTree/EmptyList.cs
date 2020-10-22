using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(BinaryTree anchor)
            : base(null,anchor, null)
            => StopByObjectIds();
        public EmptyList(BinaryTree anchorLeft, BinaryTree anchor, BinaryTree anchorRight)
            : base(anchorLeft, anchor, anchorRight)
            => StopByObjectIds();

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}