using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TokenClass
    {
        readonly Syntax _value;
        public SyntaxBoxToken(Syntax value) { _value = value; }
        protected override Syntax Terminal(SourcePart token) { return _value; }
    }
}