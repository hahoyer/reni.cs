using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken
        : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, IDeclarerTokenClass, SyntaxFactory.IDeclarerToken
    {
        BinaryTree Value { get; }
        internal ExclamationBoxToken(BinaryTree value) => Value = value;

        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
        {
            Tracer.Assert(right == null);
            return BinaryTree.Create(left, this, token, Value);
        }

        string IParserTokenType<BinaryTree>.PrioTableId => PrioTable.Any;

        SyntaxFactory.IDeclarerProvider SyntaxFactory.IDeclarerToken.Provider => SyntaxFactory.DeclarationMark;
        string ITokenClass.Id => "!";
    }
}