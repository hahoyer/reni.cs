using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal static IStructure CreateStruct(this Syntax syntax, StructFormatter parent, FormatterMode mode)
            => new Structure(syntax, parent, mode);
    }

}