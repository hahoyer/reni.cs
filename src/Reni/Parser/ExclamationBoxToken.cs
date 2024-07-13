using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser;

sealed class ExclamationBoxToken
    : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass
{
    public const string TokenId = "(!)";
    BinaryTree Value { get; }

    internal ExclamationBoxToken(BinaryTree value) => Value = value;

    BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
    {
        var leftleft = BinaryTree.Create(left, this, token, Value);
        if(right == null)
            return leftleft;

        right.Left.AssertIsNull();
        (right.TokenClass is ExclamationBoxToken).Assert();
        return right.ReCreate(T(leftleft));
    }

    string IParserTokenType<BinaryTree>.PrioTableId => TokenId;
    string ITokenClass.Id => TokenId;
}