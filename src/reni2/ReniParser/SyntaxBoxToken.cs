using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TokenClass
    {
        readonly Syntax _value;
        public SyntaxBoxToken(Syntax value) { _value = value; }
        protected override Syntax Terminal(Token token) => _value.Sourround(token.SourcePart);
        protected override Syntax Infix(Syntax left, Token token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}