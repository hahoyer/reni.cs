using System.Collections.Generic;
using hw.Scanner;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart);
        bool LineBreakScan(ref int? lineLength);
    }
}