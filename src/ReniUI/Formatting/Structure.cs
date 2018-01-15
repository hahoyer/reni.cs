using System;
using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : StructureBase
    {
        public Structure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<IEnumerable<ISourcePartEdit>>
            GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix)
            => throw new NotImplementedException();
    }
}