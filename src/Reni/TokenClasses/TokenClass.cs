using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses.Whitespace;

namespace Reni.TokenClasses;

abstract class TokenClass : ParserTokenType<BinaryTree>, ITokenClass, ISeparatorClass
{
    ContactType ISeparatorClass.ContactType
        => this is Number
            ? ContactType.AlphaNum
            : this is Text
                ? ContactType.Text
                : Lexer.IsAlphaLike(Id)
                    ? ContactType.AlphaNum
                    : Lexer.IsSymbolLike(Id)
                        ? ContactType.Symbol
                        : ContactType.Compatible;

    string ITokenClass.Id => Id;

    protected override BinaryTree Create(BinaryTree? left, IToken token, BinaryTree? right)
        => BinaryTree.Create(left, this, token, right);
}