using hw.DebugFormatter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ReniUI;

namespace ReniVSIX;

public sealed class AuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
{
    readonly ParseRequest Request;
    readonly CompilerBrowser Compiler;

    public AuthoringScope(ParseRequest request)
    {
        Request = request;
        Compiler = CompilerBrowser.FromText(request.Text, request.FileName);
    }

    public override string GetDataTipText(int line, int column, out TextSpan span)
    {
        var (result, sourcePart) = Compiler.GetDataTipText(line, column);
        span = sourcePart.ToTextSpan();
        return result;
    }

    public override Declarations GetDeclarations
        (IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
    {
        Dumpable.NotImplementedFunction("view", line, col, "info", reason);
        return default;
    }

    public override Methods GetMethods(int line, int col, string name)
    {
        Dumpable.NotImplementedFunction(line, col, name);
        return default;
    }

    public override string Goto
        (VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
    {
        span = default;
        Dumpable.NotImplementedFunction("cmd", "textView", line, col, "span");
        return default;
    }
}