using hw.DebugFormatter;
using hw.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class DeclarationTokenFactory : GenericTokenFactory<BinaryTree>
    {
        public DeclarationTokenFactory(string title)
            : base(title) { }

        protected override IParserTokenType<BinaryTree> GetTokenClass(string name)
            => new InvalidDeclarationError(name);
    }

    sealed class InvalidDeclarationError : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, IDeclarationTag
    {
        readonly string Name;
        public InvalidDeclarationError(string name) => Name = name;

        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);

        string IParserTokenType<BinaryTree>.PrioTableId => Name;
        string ITokenClass.Id => Name;
    }
}