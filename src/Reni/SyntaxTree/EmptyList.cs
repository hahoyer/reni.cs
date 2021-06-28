using Reni.Basics;
using Reni.Context;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(Anchor anchor, Issue issue = null)
            : base(anchor, issue)
        {
            StopByObjectIds();
            AssertValid();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}