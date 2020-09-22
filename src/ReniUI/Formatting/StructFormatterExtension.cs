using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static IStructure CreateStruct(this Reni.TokenClasses.Syntax syntax, Context context)
            => CreateStruct(new Syntax(syntax, null), context);
        internal static IStructure CreateStruct(this Syntax syntax, Context context)
            => new Structure(syntax, context);
    }
}