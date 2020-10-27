namespace Reni.SyntaxTree
{
    sealed class CleanupSyntax : Syntax
    {
        internal readonly ValueSyntax Value;

        public CleanupSyntax(ValueSyntax value)
            : base(anchor: SyntaxTree.Anchor.Create())
            => Value = value;

        protected override int LeftDirectChildCountInternal => 0;
        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index) => Value;
    }
}