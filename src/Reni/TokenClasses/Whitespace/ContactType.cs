using Reni.Parser;

namespace Reni.TokenClasses.Whitespace
{
    sealed class ContactType
    {
        internal static readonly ContactType AlphaNum = new ContactType();
        internal static readonly ContactType Text = new ContactType();
        internal static readonly ContactType Incompatible = new ContactType();
        internal static readonly ContactType Compatible = new ContactType();

        internal bool IsCompatible(ContactType other)
        {
            if(this == Compatible)
                return true;
            if(other == Compatible)
                return true;

            if(this == Incompatible)
                return false;
            if(other == Incompatible)
                return false;

            return this != other;
        }

        internal static ContactType Get(ISeparatorClass target)
            => target == null
                ? Compatible
                : target.ContactType;
    }
}