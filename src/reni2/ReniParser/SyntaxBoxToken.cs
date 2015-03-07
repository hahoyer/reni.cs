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
        protected override Syntax Terminal(SourcePart token) => _value.Sourround(token);
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}