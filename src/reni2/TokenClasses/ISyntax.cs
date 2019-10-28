using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses
{
    public interface ISyntax
    {
        [DisableDump]
        SourcePart All { get; }
        [DisableDump]
        SourcePart Main { get; }
    }
}