using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class SourceWrapper : Microsoft.VisualStudio.Package.Source
    {
        internal Source Data;

        public SourceWrapper(LanguageService parent, IVsTextLines buffer)
            : base(parent, buffer, null) { OnChange(); }

        void OnChange() { Data = GetTextLines().CreateReniSource(); }

        public override void OnChangeLineText(TextLineChange[] lineChange, int last)
        {
            base.OnChangeLineText(lineChange, last);
            OnChange();
        }

        public override void ReformatSpan(EditArray mgr, TextSpan span)
        {
            var ca = new CompoundAction(this, "Reformat code");
            using(ca)
            {
                ca.FlushEditActions();
                Data.ReformatSpan(mgr, span);
            }
        }

        public override TokenInfo GetTokenInfo(int line, int col) => Data.GetTokenInfo(line, col);
    }
}