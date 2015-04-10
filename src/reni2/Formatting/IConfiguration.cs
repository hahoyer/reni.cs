using System.Collections.Generic;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface IConfiguration
    {
        ISubConfiguration Assess(BinaryTree binaryTree);
    }

    interface ISubConfiguration
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

        string Default
            (
            string left,
            IEnumerable<WhiteSpaceToken> head,
            string token,
            IEnumerable<WhiteSpaceToken> tail,
            string right
            );

        bool IsMultiline { get; }
        int? MaxListItemLength(List list);
        int? MaxListLength(List list);

        string Reformat(BinaryTree target, IConfiguration configuration);
    }
}