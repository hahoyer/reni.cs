using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ReniVSIX;

sealed class SourceWrapper : Microsoft.VisualStudio.Package.Source
{
    internal Source Data;
    readonly ReniService Parent;

    public SourceWrapper(ReniService parent, IVsTextLines buffer)
        : base(parent, buffer, null)
    {
        Parent = parent;
        OnChange();
    }

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
            Data.ReformatSpan(mgr, span, Parent.Package.CreateFormattingProvider());
        }
    }

    public override TokenInfo GetTokenInfo(int line, int col) => Data.GetTokenInfo(line, col);

    void OnChange() => Data = GetTextLines().CreateReniSource();
}