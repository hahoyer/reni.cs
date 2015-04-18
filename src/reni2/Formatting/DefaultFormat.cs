using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class DefaultFormat : DumpableObject, IConfiguration
    {
        internal const int MaxLineLength = 100;
        public static readonly DefaultFormat Instance = new DefaultFormat();

        DefaultFormat() { }

        string IConfiguration.Reformat(ITreeItem target)
            => Reformat(target);

        internal string Reformat(ITreeItem target)
        {
            if(target == null)
                return null;

            var separator = target.UseLength(MaxLineLength) > 0
                ? SeparatorType.Contact
                : SeparatorType.Multiline;

            return target.Reformat(this, separator);
        }

        string IConfiguration.Reformat(ListTree target, ISeparatorType separator)
        {
            var targets =
                target
                    .Items
                    .Select(item => item.Reformat(this) ?? "")
                    .ToArray();

            var listSeparator = separator == SeparatorType.Multiline
                ? separator
                : SeparatorType.Close;

            return PrettyLines(targets).Stringify(listSeparator.Text);
        }

        static IEnumerable<string> PrettyLines(IEnumerable<string> lines)
        {
            bool? formerIsMultiline = null;

            foreach(var line in lines)
            {
                var isMultiline = line.Any(item => item == '\n');

                if(formerIsMultiline == null)
                    yield return line;
                else if(formerIsMultiline.Value || isMultiline)
                    yield return "\n" + line;
                else
                    yield return line;

                formerIsMultiline = isMultiline;
            }
        }

        string IConfiguration.Reformat(BinaryTree target, ISeparatorType separator)
        {
            var left = Reformat(target.Left);
            var leftSeparator = target.LeftInnerSeparator();
            var token = target.Token.Id;
            var rightSeparator = target.RightInnerSeparator();
            var right = Reformat(target.Right);

            return (left == null ? "" : (left + leftSeparator.Text)) +
                token +
                (right == null ? "" : (rightSeparator.Text + right));
        }

        internal static ISeparatorType Separator(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ??
                BaseSeparatorType(left, right);

        static ISeparatorType BaseSeparatorType(ITokenClass left, ITokenClass right)
            => ContactClass(left).IsCompatible(ContactClass(right))
                ? SeparatorType.Contact
                : SeparatorType.Close;

        static ISeparatorType PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if (right is Colon || right is EndToken)
                return null;

            if (left is List)
                return SeparatorType.Close;

            if (left is Colon && right is LeftParenthesis)
                return SeparatorType.Close;

            if(left is LeftParenthesis || right is RightParenthesis)
                return SeparatorType.Contact;

            return SeparatorType.Close;
        }

        static ContactType ContactClass(ITokenClass target)
            => target == null
                ? ContactType.Compatible
                : ReniLexer.IsAlphaLike(target.Id) || target is Number
                    ? ContactType.AlphaNum
                    : target is Text
                        ? ContactType.Text
                        : (ReniLexer.IsSymbolLike(target.Id)
                            ? ContactType.Symbol
                            : ContactType.Compatible);
    }
}