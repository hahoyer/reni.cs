using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio.Package;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class DummyScanner : DumpableObject, IScanner
    {
        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state) => false;
        void IScanner.SetSource(string source, int offset) { }
    }
}