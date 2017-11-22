using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class SingleItemStructure : Structure
    {
        public SingleItemStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent)
        {
            Tracer.Assert(Syntax.Left == null);
            Tracer.Assert(Syntax.Right == null);
        }

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart)
            => FormatterToken.Create(Syntax).Select(item => item.ToSourcePartEdit());
    }
}