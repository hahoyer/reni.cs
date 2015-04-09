using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class DefaultFormat : DumpableObject, IConfiguration
    {
        public static readonly IConfiguration Instance = new DefaultFormat();

        DefaultFormat() { }

        string IConfiguration.Parenthesis
            (
            IEnumerable<WhiteSpaceToken> leftHead,
            string left,
            IEnumerable<WhiteSpaceToken> leftTail,
            string content,
            IEnumerable<WhiteSpaceToken> rightHead,
            string right,
            IEnumerable<WhiteSpaceToken> rightTail)
        {
            if(
                new[] {leftHead, leftTail, rightHead}
                    .SelectMany(i => i)
                    .IsRelevantWhitespace()
                )
                NotImplementedMethod
                    (
                        leftHead.ToArray(),
                        left,
                        leftTail.ToArray(),
                        content,
                        rightHead.ToArray(),
                        right,
                        rightTail.ToArray()
                    );

            if(rightTail.HasComment())
                NotImplementedMethod
                    (
                        leftHead.ToArray(),
                        left,
                        leftTail.ToArray(),
                        content,
                        rightHead.ToArray(),
                        right,
                        rightTail.ToArray()
                    );

            return left + content + right + (rightTail.SourcePart()?.Id ?? "");
        }

        string IConfiguration.DeclarationItem
            (SourceSyntax item, IEnumerable<WhiteSpaceToken> tail)
            => item.Reformat(tail, this);

        string IConfiguration.Default
            (
            string left,
            IEnumerable<WhiteSpaceToken> head,
            string token,
            IEnumerable<WhiteSpaceToken> tail,
            string right
            )
        {
            if((head.IsRelevantWhitespace()) || (tail.IsRelevantWhitespace()))
                return (left ?? "") +
                    (head.SourcePart()?.Id ?? "") +
                    token +
                    (tail.SourcePart()?.Id ?? "") +
                    (right ?? "");

            return (left == null ? "" : left + " ") + token + (right == null ? "" : " " + right);
        }

        string IConfiguration.Exclamation(IToken token, string right) => token.Id + right;
        bool IConfiguration.IsMultiline => false;
        IConfiguration IConfiguration.SingleLine => this;
        int? IConfiguration.MaxListItemLength(List list) => null;
        int? IConfiguration.MaxListLength(List list) => null;
    }
}