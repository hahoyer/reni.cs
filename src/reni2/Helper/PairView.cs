using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Helper
{
    abstract class ProxySyntax : Syntax
    {
        internal class RightBracket : ProxySyntax
        {
            public RightBracket(Syntax client, BinaryTree anchor)
                : base(client, anchor)
                => (anchor.TokenClass is IRightBracket).Assert();
        }

        internal class LeftBracket : ProxySyntax
        {
            public LeftBracket(Syntax client, BinaryTree anchor)
                : base(client, anchor)
                => (anchor.TokenClass is ILeftBracket).Assert();
        }

        [DisableDump]
        internal readonly Syntax Client;

        ProxySyntax(Syntax client, BinaryTree anchor)
            : base(anchor)
            => Client = client;

        [DisableDump]
        internal override int LeftDirectChildCount => Client.LeftDirectChildCount;

        [DisableDump]
        protected override int DirectChildCount => Client.LeftDirectChildCount;

        protected override Syntax GetDirectChild(int index) => Client.DirectChildren[index];
    }
}