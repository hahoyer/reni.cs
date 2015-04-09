using System.Collections.Generic;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface IConfiguration
    {
        string Parenthesis
            (
            IEnumerable<WhiteSpaceToken> leftHead,
            string left,
            IEnumerable<WhiteSpaceToken> leftTail,
            string content,
            IEnumerable<WhiteSpaceToken> rightHead,
            string right,
            IEnumerable<WhiteSpaceToken> rightTail);

        string DeclarationItem(SourceSyntax item, IEnumerable<WhiteSpaceToken> tail);

        string Default
            (
            string left,
            IEnumerable<WhiteSpaceToken> head,
            string token,
            IEnumerable<WhiteSpaceToken> tail,
            string right
            );

        string Exclamation(IToken token, string right);
        bool IsMultiline { get; }
        IConfiguration SingleLine { get; }
        int? MaxListItemLength(List list);
        int? MaxListLength(List list);
    }
}