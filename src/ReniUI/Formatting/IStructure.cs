using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix, bool includeSuffix);
        Syntax Syntax {get;}
    }
}