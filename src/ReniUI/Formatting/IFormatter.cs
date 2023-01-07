using hw.Scanner;

namespace ReniUI.Formatting;

interface IFormatter
{
    IEnumerable<Edit> GetEditPieces(CompilerBrowser compiler, SourcePart targetPart);
}