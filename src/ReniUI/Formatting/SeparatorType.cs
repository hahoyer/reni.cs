using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    interface ISeparatorType
    {
        string Text {get;}
    }

    static class SeparatorType
    {
        sealed class Contact : DumpableObject, ISeparatorType
        {
            string ISeparatorType.Text => "";
        }

        sealed class Close : DumpableObject, ISeparatorType
        {
            string ISeparatorType.Text => " ";
        }

        internal static readonly ISeparatorType ContactSeparator = new Contact();
        internal static readonly ISeparatorType CloseSeparator = new Close();

        internal static ISeparatorType Get(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ?? BaseSeparatorType(left, right);

        static ISeparatorType BaseSeparatorType(ITokenClass left, ITokenClass right)
            => ContactType.Get(left).IsCompatible(ContactType.Get(right))
                ? ContactSeparator
                : CloseSeparator;

        static ISeparatorType PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if(left == null || right == null)
                return null;
            if((left is List || left is Colon) && !(right is RightParenthesis))
                return CloseSeparator;

            if(right is RightParenthesis ||
               right is LeftParenthesis ||
               right is List ||
               right is EndOfText ||
               left is LeftParenthesis || 
               left is BeginOfText
            )
                return ContactSeparator;

            if(right is Colon || left is ExclamationBoxToken)
                return null;

            return CloseSeparator;
        }
    }
}