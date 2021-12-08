using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken
        : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass
    {
        BinaryTree Value { get; }

        internal ExclamationBoxToken(BinaryTree value) => Value = value;

        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
        {
            (right == null).Assert();
            return BinaryTree.Create(left, this, token, Value);
        }

        string IParserTokenType<BinaryTree>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";
    }
}