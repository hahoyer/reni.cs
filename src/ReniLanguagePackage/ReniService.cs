using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    [UsedImplicitly]
    public sealed class ReniService : LanguageService
    {
        sealed class AuthoringScopeWrapper : AuthoringScope
        {
            readonly ReniAuthoringScope _data;

            internal AuthoringScopeWrapper(ParseRequest request)
            {
                _data = new ReniAuthoringScope(request);
            }

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

        readonly ValueCache<LanguagePreferences> _preferencesCache;

        public ReniService()
        {
            _preferencesCache = new ValueCache<LanguagePreferences>
                (() => new LanguagePreferences(Site, typeof(ReniService).GUID, Name));
        }

        public override string Name => "Reni";

        public override LanguagePreferences GetLanguagePreferences() 
            => _preferencesCache.Value;

        public override IScanner GetScanner(IVsTextLines buffer) 
            => new ReniScanner(buffer);

        public override AuthoringScope ParseSource(ParseRequest request)
            => new AuthoringScopeWrapper(request);

        public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";
    }
}