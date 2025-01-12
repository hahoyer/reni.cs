using hw.DebugFormatter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni.DeclarationOptions;
using ReniUI;

namespace ReniVSIX;

sealed class Declarations : Microsoft.VisualStudio.Package.Declarations
{
    readonly Declaration[] Value;
    public Declarations(Declaration[] value) => Value = value;

    public override int GetCount() => Value.Length;
    public override string GetDisplayText(int index) => Value[index].Name;
    public override string GetName(int index) => Value[index].Name;
    public override string GetDescription(int index) => Value[index].Name;
    public override int GetGlyph(int index) => -1;
}

public sealed class AuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
{
    readonly CompilerBrowser Compiler;

    public AuthoringScope(ParseRequest request)
        => Compiler = CompilerBrowser.FromText(request.Text, request.FileName);

    public override string GetDataTipText(int line, int column, out TextSpan span)
    {
        var (result, sourcePart) = Compiler.GetDataTipText(line, column);
        span = sourcePart.ToTextSpan();
        return result;
    }

    public override Microsoft.VisualStudio.Package.Declarations GetDeclarations
        (IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
    {
        var declarations = Compiler
            .GetDeclarations((Compiler.Source + info.StartIndex).Span(Compiler.Source + info.EndIndex));
        return new Declarations(declarations);
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