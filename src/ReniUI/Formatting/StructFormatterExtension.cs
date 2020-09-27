namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static Syntax CreateStruct(this Reni.TokenClasses.Syntax syntax)
            => new Syntax(syntax);
    }
}