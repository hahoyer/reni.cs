using hw.DebugFormatter;
using hw.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken
        : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, IDeclarerToken
    {
        BinaryTree Value { get; }
        internal ExclamationBoxToken(BinaryTree value) => Value = value;

        IDeclarerProvider IDeclarerToken.Provider => Factory.DeclarationMark;

        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
        {
            (right == null).Assert();
            return BinaryTree.Create(left, this, token, Value);
        }

        string IParserTokenType<BinaryTree>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";
    }
}