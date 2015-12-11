using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ColorizerWrapper : Colorizer
    {
        readonly LanguageService _parent;
        readonly IVsTextLines _buffer;
        const bool Trace = false;

        public ColorizerWrapper(LanguageService parent, IVsTextLines buffer)
            : base(parent, buffer, null)
        {
            _parent = parent;
            _buffer = buffer;
        }

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
            => Source.StateAtEndOfLine(line, Trace).GetHashCode();

        Source Source => ((SourceWrapper) _parent.GetOrCreateSource(_buffer)).Data;

        public override int ColorizeLine
            (int line, int length, IntPtr ptr, int state, uint[] attrs)
        {
            Source.ColorizeLine(line, attrs, Trace);
            return Source.StateAtEndOfLine(line, Trace).GetHashCode();
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