using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class SeparatorExtension
    {
        internal static bool Get(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ?? BaseSeparatorType(left, right);

        static bool BaseSeparatorType(ITokenClass left, ITokenClass right)
            => !ContactType.Get(left).IsCompatible(ContactType.Get(right));

        static bool? PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if(left == null || right == null)
                return null;
            if((left is List || left is Colon) && !(right is RightParenthesis))
                return true;

            if(right is RightParenthesis ||
               right is LeftParenthesis ||
               right is List ||
               right is EndOfText ||
               left is LeftParenthesis || 
               left is BeginOfText
            )
                return false;

            if(right is Colon || left is ExclamationBoxToken)
                return null;

            return true;
        }
    }
}