using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart);
        bool LineBreakScan(ref int? lineLength);
        Syntax Syntax {get;}
    }
}