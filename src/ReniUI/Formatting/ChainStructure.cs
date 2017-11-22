using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ChainStructure : Structure
    {
        public ChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart)
        {
            if(Syntax.Left == null && Syntax.Right == null)
                return FormatterToken.Create(Syntax).Select(item => item.ToSourcePartEdit());

            NotImplementedMethod(targetPart);
            return null;
        }
    }
}

    