using System.Collections.Generic;

namespace ReniUI.Formatting
{
    interface IStructure
    {
        IEnumerable<ISourcePartEdit> Edits {get;}
        int LineBreaks {get;}
    }
}