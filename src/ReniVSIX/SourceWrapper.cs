using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ReniUI.Formatting;

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
            var configuration = Parent
                .Package
                .GetFormattingConfiguration();
            configuration.LineBreakString = GetLineBreakString();
            var formattingProvider = configuration.Create();
            Data.ReformatSpan(mgr, span, formattingProvider);
        }
    }

    public override TokenInfo GetTokenInfo(int line, int col) => Data.GetTokenInfo(line, col);

    public override void Completion(IVsTextView textView, TokenInfo info, ParseReason reason)
    {
        Dumpable.NotImplementedFunction("textView", "info", reason);
        base.Completion(textView, info, reason);
    }

    string GetLineBreakString()
        => GetLineCount()
                .Select(GetNewLine)
                .GroupBy(text => text)
                .Select(group => (text: group.Key, count: group.Count()))
                .OrderByDescending(group => group.count)
                .FirstOrDefault().text ??
            "\n";

    void OnChange()
    {
        var text = GetTextLines();
        Data = text.CreateReniSource();
    }
}