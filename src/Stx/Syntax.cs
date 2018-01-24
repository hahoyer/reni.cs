using System;
using hw.Parser;
using hw.Scanner;

namespace Stx
{
    class Syntax : ISourcePartProxy
    {
        readonly SourcePart SourcePart;
        public Syntax(SourcePart sourcePart) => SourcePart = sourcePart;
        SourcePart ISourcePartProxy.All => SourcePart;
        public Syntax Left => throw new NotImplementedException();
        public Syntax Right => throw new NotImplementedException();
        public static Syntax CreateSourceSyntax(Syntax left, TokenClass tokenClass, IToken token, Syntax right) {throw new NotImplementedException();}
    }
}