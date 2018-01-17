using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting {
    sealed class DefinableChainStructure : StructureBase
    {
        public DefinableChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<ISourcePartEdit> GetMainAndSiteEdits(SourcePart targetPart, bool includeSuffix)
            => base.GetMainAndSiteEdits(targetPart, includeSuffix).IndentRight();
    }
}