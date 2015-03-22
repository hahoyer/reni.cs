using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni.UserInterface;

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

        sealed class ColorizerWrapper : Colorizer
        {
            readonly ReniColorizer _data;

            public ColorizerWrapper(LanguageService svc, IVsTextLines buffer)
                : base(svc, buffer, null)
            {
                _data = new ReniColorizer(buffer);
            }

            public override int GetStateMaintenanceFlag(out int value)
            {
                value = 1;
                return VSConstants.S_OK;
            }

            public override int GetStartState(out int start)
            {
                start = ReniColorizer.StartState.ObjectId;
                return VSConstants.S_OK;
            }

            public override int GetStateAtEndOfLine(int line, int length, IntPtr ptr, int state)
                => _data.StateAtEndOfLine(line).ObjectId;

            public override int ColorizeLine
                (int line, int length, IntPtr ptr, int state, uint[] attrs)
            {
                _data.ColorizeLine(line, attrs);
                return _data.StateAtEndOfLine(line).ObjectId;
            }

            public override int GetColorInfo(string line, int length, int state)
            {
                Tracer.TraceBreak();
                return 0;

            }

            public override TokenInfo[] GetLineInfo
                (IVsTextLines buffer, int line, IVsTextColorState colorState) => null;
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

        public override Colorizer GetColorizer(IVsTextLines buffer)
            => new ColorizerWrapper(this, buffer);

        public override IScanner GetScanner(IVsTextLines buffer) => null;

        public override AuthoringScope ParseSource(ParseRequest request)
            => new AuthoringScopeWrapper(request);

        public override string GetFormatFilterList() => "Reni files (*.reni)\n*.reni\n";
    }
}