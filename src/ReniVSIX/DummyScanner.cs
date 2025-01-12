using hw.DebugFormatter;
using Microsoft.VisualStudio.Package;

namespace ReniVSIX;

sealed class DummyScanner : DumpableObject, IScanner
{
    public static readonly IScanner Instance = new DummyScanner();
    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state) => false;
    void IScanner.SetSource(string source, int offset) { }
}