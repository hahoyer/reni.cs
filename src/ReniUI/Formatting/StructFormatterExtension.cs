using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static IStructure CreateStruct(this Syntax syntax, Context context)
            => new Structure(syntax, context);
    }

}