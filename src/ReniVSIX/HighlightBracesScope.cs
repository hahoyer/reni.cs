using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ReniUI;

namespace ReniVSIX;

public sealed class HighlightBracesScope : AuthoringScope
{
    readonly CompilerBrowser Compiler;

    public HighlightBracesScope(ParseRequest request)
        => Compiler = CompilerBrowser.FromText(request.Text, request.FileName);

    public override string GetDataTipText(int line, int column, out TextSpan span)
    {
        var (result, sourcePart) = Compiler.GetDataTipText(line, column);
        span = sourcePart.ToTextSpan();
        return result;
    }

    public override Declarations GetDeclarations
        (IVsTextView view, int line, int col, TokenInfo info, ParseReason reason) => null;

    public override Methods GetMethods(int line, int col, string name) => null;

    public override string Goto
        (VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
    {
        span = default;
        return null;
    }
}