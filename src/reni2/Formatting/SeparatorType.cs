using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
        internal static readonly ISeparatorType None = new NoneType();
        internal static readonly ISeparatorType Contact = new ConcatType("");
        internal static readonly ISeparatorType Close = new ConcatType(" ");

        sealed class NoneType : DumpableObject, ISeparatorType
        {
            string ISeparatorType.Text => null;
        }

        sealed class ConcatType : DumpableObject, ISeparatorType
        {
            readonly string _separator;
            internal ConcatType(string separator) { _separator = separator; }

            string ISeparatorType.Text => _separator;
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
            if(left is List && right is LeftParenthesis)
                return Close;

            if(right is RightParenthesis ||
                right is LeftParenthesis ||
                right is List ||
                left is LeftParenthesis)
                return Contact;

            if(right is Colon || left is ExclamationBoxToken)
                return null;

            return Close;
        }
    }
}