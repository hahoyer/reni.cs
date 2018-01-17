using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting {
    sealed class ColonStructure : StructureBase
    {
        public ColonStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<ISourcePartEdit> GetLeftSiteEdits(SourcePart targetPart, bool exlucdePrefix)
            => base.GetLeftSiteEdits(targetPart, exlucdePrefix).IndentLeft();
    }
}