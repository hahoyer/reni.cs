using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ScannerWrapper : DumpableObject, IScanner
    {
        readonly ReniScanner _data;
        public ScannerWrapper(IVsTextLines buffer) { _data = new ReniScanner(buffer); }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
            => _data.ScanTokenAndProvideInfoAboutIt(tokenInfo, ref state);

        void IScanner.SetSource(string source, int offset) => _data.SetSource(source, offset);
    }

    sealed class ReniScanner : DumpableObject
    {
        readonly IVsTextLines _buffer;

        public ReniScanner(IVsTextLines buffer) { _buffer = buffer; }
        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            NotImplementedMethod(tokenInfo, state);
            return false;
        }
        public void SetSource(string source, int offset) { NotImplementedMethod(source, offset); }
    }
}