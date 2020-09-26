namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static Syntax CreateStruct(this Reni.TokenClasses.Syntax syntax, Configuration configuration)
            => new Syntax(syntax, configuration);
    }
}