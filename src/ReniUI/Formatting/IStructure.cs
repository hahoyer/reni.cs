using System.Collections.Generic;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        (IEnumerable<ISourcePartEdit> edits,int endLineBreaks) Get(int minimalLineBreaks);
        Syntax Syntax {get;}
    }
}