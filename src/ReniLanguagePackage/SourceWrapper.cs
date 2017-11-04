using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class SourceWrapper : Microsoft.VisualStudio.Package.Source
    {
        readonly ReniService _parent;
        internal Source Data;

        public SourceWrapper(ReniService parent, IVsTextLines buffer)
            : base(parent, buffer, null)
        {
            _parent = parent;
            OnChange();
        }

        void OnChange() {Data = GetTextLines().CreateReniSource();}

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
                Data.ReformatSpan(mgr, span, _parent.Package.CreateFormattingProvider());
            }
        }

        public override TokenInfo GetTokenInfo(int line, int col) => Data.GetTokenInfo(line, col);
    }
}