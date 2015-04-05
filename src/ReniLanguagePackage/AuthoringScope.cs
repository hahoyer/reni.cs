using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class AuthoringScope : DumpableObject
    {
        public string GetDataTipText(int line, int col, out TextSpan span)
        {
            span = new TextSpan();
            NotImplementedMethod(line, col, span);
            return null;
        }
        public Declarations GetDeclarations
            (IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            NotImplementedMethod(view, line, col, info, reason);
            return null;
        }
        public Methods GetMethods(int line, int col, string name)
        {
            NotImplementedMethod(line, col, name);
            return null;
        }

        public string Goto
            (
            VSConstants.VSStd97CmdID cmd,
            IVsTextView textView,
            int line,
            int col,
            out TextSpan span)
        {
            span = new TextSpan();
            NotImplementedMethod(cmd, textView, line, col, span);
            return null;
        }
    }
}