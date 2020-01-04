using System.Collections.Generic;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        IEnumerable<ISourcePartEdit> GetSourcePartEdits(bool excludePrefix, bool includeSuffix);
        Syntax Syntax {get;}
    }
}