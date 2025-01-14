namespace Reni.TokenClasses.Whitespace;

sealed class ContactType
{
    [UsedImplicitly]
    readonly string Tag;
    internal static readonly ContactType AlphaNum = new ContactType(nameof(AlphaNum));
    internal static readonly ContactType Text = new ContactType(nameof(Text));
    internal static readonly ContactType Incompatible = new ContactType(nameof(Incompatible));
    internal static readonly ContactType Compatible = new ContactType(nameof(Compatible));
    internal static readonly ContactType Symbol = new ContactType(nameof(Symbol));

    ContactType(string tag) => Tag = tag;

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

    internal static ContactType Get(ISeparatorClass? target)
        => target == null
            ? Compatible
            : target.ContactType;
}