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
        readonly SourceSyntax _value;
        public SyntaxBoxToken(SourceSyntax value) { _value = value; }

        protected override Syntax Terminal(SourcePart token) => _value.Syntax;

        protected override Syntax Infix
            (Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string Id => "<box>";
    }
}