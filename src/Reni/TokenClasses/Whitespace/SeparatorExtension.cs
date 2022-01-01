using JetBrains.Annotations;
using Reni.Parser;

namespace Reni.TokenClasses.Whitespace
{
    static class SeparatorExtension
    {
        [PublicAPI]
        internal static bool Get(ITokenClass left, ITokenClass right)
            => Get(left as ISeparatorClass, right as ISeparatorClass);

        [PublicAPI]
        internal static bool Get(ITokenClass left, ISeparatorClass right)
            => Get(left as ISeparatorClass, right);

        [PublicAPI]
        internal static bool Get(ISeparatorClass left, ITokenClass right)
            => Get(left, right as ISeparatorClass);

        [PublicAPI]
        internal static bool Get(ISeparatorClass left, ISeparatorClass right)
            => PrettySeparatorType(left, right) ?? BaseSeparatorType(left, right);

        static bool BaseSeparatorType(ISeparatorClass left, ISeparatorClass right)
            => !ContactType.Get(left).IsCompatible(ContactType.Get(right));

        static bool? PrettySeparatorType(ISeparatorClass left, ISeparatorClass right)
        {
            if(left == null || right == null)
                return null;
            if(left is BeginOfText || right is EndOfText)
                return false;
            if((left is List || left is Colon) && !(right is IRightBracket))
                return true;

            if(right is IRightBracket ||
               right is ILeftBracket||
               right is List ||
               left is ILeftBracket
              )
                return false;

            if(right is Colon)
                return null;

            return true;
        }
    }
}