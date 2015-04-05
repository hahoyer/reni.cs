using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class AuthoringScopeWrapper : Microsoft.VisualStudio.Package.AuthoringScope
    {
        readonly AuthoringScope _data;

        internal AuthoringScopeWrapper() { _data = new AuthoringScope(); }

        public override string GetDataTipText(int line, int col, out TextSpan span)
            => _data.GetDataTipText(line, col, out span);

        public override Declarations GetDeclarations
            (IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
            => _data.GetDeclarations(view, line, col, info, reason);

        public override Methods GetMethods(int line, int col, string name)
            => _data.GetMethods(line, col, name);

        public override string Goto
            (
            VSConstants.VSStd97CmdID cmd,
            IVsTextView textView,
            int line,
            int col,
            out TextSpan span)
            => _data.Goto(cmd, textView, line, col, out span);
    }
}