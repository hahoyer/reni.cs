using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;
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

        internal static bool IsLinebreakCandidate(ITokenClass left, ITokenClass right)
        {
            if(left == null
                || right is Colon
                || right is List
                || right is TokenClasses.Function
                || right is EndToken
                )
                return false;

            if(right is LeftParenthesis
                || right is RightParenthesis
                || left is LeftParenthesis
                || left is RightParenthesis
                )
                return true;

            if(left is List)
                return true;

            if(IsOperator(right))
                if(IsOperator(left)
                    || left is Colon
                    || left is TokenClasses.Function
                    )
                    return false;

            Dumpable.NotImplementedFunction(left, right);

            return false;
        }

        static bool IsOperator(ITokenClass tokenClass)
            => tokenClass is Number
                || tokenClass is Text
                || tokenClass is TypeOperator
                || tokenClass is Definable
                || tokenClass is ExclamationBoxToken
                || tokenClass is MutableDeclarationToken
                || tokenClass is ArgToken
                || tokenClass is InstanceToken;
    }
}