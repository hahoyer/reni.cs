using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TokenClass : ParserTokenType<BinaryTree>, ITokenClass, ISyntaxFactoryToken
    {
        [DisableDump]
        internal virtual bool IsVisible => true;

        [DisableDump]
        protected virtual ISyntaxFactory Provider
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        ISyntaxFactory ISyntaxFactoryToken.Provider => Provider;

        string ITokenClass.Id => Id;

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);
    }
}