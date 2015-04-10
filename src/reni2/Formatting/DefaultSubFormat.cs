using System;
using System.Collections.Generic;
using hw.Parser;
using System.Linq;
using hw.Debug;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class DefaultSubFormat : DumpableObject, ISubConfiguration
    {
        public static readonly ISubConfiguration Instance = new DefaultSubFormat();

        DefaultSubFormat() { }

        string ISubConfiguration.Parenthesis
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

        string ISubConfiguration.Default
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

        bool ISubConfiguration.IsMultiline => false;
        int? ISubConfiguration.MaxListItemLength(List list) => null;
        int? ISubConfiguration.MaxListLength(List list) => null;

        string ISubConfiguration.Reformat(BinaryTree target, IConfiguration configuration)
            => Reformat(target);

        static string Reformat(BinaryTree target)
        {
            if(target == null)
                return "";

            var left = Reformat(target.Left);
            var token =
                (target.TokenHead.SourcePart()?.Id ?? "") +
                    target.Token +
                    (target.TokenTail.SourcePart()?.Id ?? "");
            var right = Reformat(target.Right);
            return left + token + right;
        }
    }
}