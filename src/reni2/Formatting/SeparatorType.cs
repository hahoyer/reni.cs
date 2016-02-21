using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface ISeparatorType
    {
        string Text { get; }
    }

    static class SeparatorType
    {
        static readonly ISeparatorType Contact = new ConcatType("");
        static readonly ISeparatorType Close = new ConcatType(" ");

        [DebuggerDisplay("{Separator}")]
        sealed class ConcatType : DumpableObject, ISeparatorType
        {
            readonly string Separator;
            internal ConcatType(string separator) { Separator = separator; }

            string ISeparatorType.Text => Separator;
        }

        internal static ISeparatorType Get(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ?? BaseSeparatorType(left, right);

        static ISeparatorType BaseSeparatorType(ITokenClass left, ITokenClass right)
            => ContactType.Get(left).IsCompatible(ContactType.Get(right))
                ? Contact
                : Close;

        static ISeparatorType PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if(left == null || right == null)
                return null;
            if((left is List || left is Colon) && !(right is RightParenthesis))
                return Close;

            if(right is RightParenthesis ||
                right is LeftParenthesis ||
                right is RightParenthesis.Matched ||
                right is List ||
                right is EndToken ||
                left is LeftParenthesis ||
                left is BeginToken ||
                left is RightParenthesis.Matched
                )
                return Contact;

            if(right is Colon || left is ExclamationBoxToken)
                return null;

            return Close;
        }
    }
}