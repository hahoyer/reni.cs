using System.Collections.Generic;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public interface IFormatter
    {
        IEnumerable<Edit> GetEditPieces(CompilerBrowser compiler, SourcePart targetPart);
    }
}