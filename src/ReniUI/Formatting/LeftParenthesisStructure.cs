using System.Collections.Generic;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting {
    sealed class LeftParenthesisStructure : StructureBase
    {
        public LeftParenthesisStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<ISourcePartEdit> GetRightSiteEdits(SourcePart targetPart, bool includeSuffix)
            => base.GetRightSiteEdits(targetPart, includeSuffix).IndentRight();
    }
}