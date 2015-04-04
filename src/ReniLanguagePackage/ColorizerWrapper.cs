using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ColorizerWrapper : Colorizer
    {
        const bool Trace = true;

        readonly Source _source;

        public ColorizerWrapper(LanguageService svc, IVsTextLines buffer, Source source)
            : base(svc, buffer, null) { _source = source; }

        public override int GetStateMaintenanceFlag(out int value)
        {
            value = 1;
            return VSConstants.S_OK;
        }

        public override int GetStartState(out int start)
        {
            start = "".GetHashCode();
            return VSConstants.S_OK;
        }

        public override int GetStateAtEndOfLine(int line, int length, IntPtr ptr, int state)
            => _source.StateAtEndOfLine(line, Trace).GetHashCode();

        public override int ColorizeLine
            (int line, int length, IntPtr ptr, int state, uint[] attrs)
        {
            _source.ColorizeLine(line, attrs, Trace);
            return _source.StateAtEndOfLine(line, Trace).GetHashCode();
        }

        public override int GetColorInfo(string line, int length, int state)
        {
            Tracer.TraceBreak();
            return 0;
        }

        public override TokenInfo[] GetLineInfo
            (IVsTextLines buffer, int line, IVsTextColorState colorState)
        {
            Tracer.TraceBreak();
            return null;
        }
    }
}