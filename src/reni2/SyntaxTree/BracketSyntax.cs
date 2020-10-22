using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    class BracketSyntax : ValueSyntax
    {
        class LeftBracket : Syntax.NoChildren
        {
            public LeftBracket(BinaryTree anchor)
                : base(anchor) { }
        }

        readonly ValueSyntax Kernel;

        readonly LeftBracket Left;

        public BracketSyntax(BinaryTree leftAnchor, ValueSyntax kernel, BinaryTree rightAnchor)
            : base(rightAnchor)
        {
            Left = new LeftBracket(leftAnchor);
            Kernel = kernel;
        }

        internal override int LeftDirectChildCount => 2;
        protected override int DirectChildCount => 2;

        protected override Syntax GetDirectChild(int index) => index switch
        {
            0 => Left, 1 => Kernel, _ => null
        };
    }
}