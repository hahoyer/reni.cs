using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static IStructure CreateStruct(this Reni.TokenClasses.Syntax syntax, Context context)
            => CreateStruct(new Helper.Syntax(syntax), context);
        internal static IStructure CreateStruct(this Helper.Syntax syntax, Context context)
            => new Structure(syntax, context);
    }
}