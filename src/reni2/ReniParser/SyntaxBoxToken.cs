using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TerminalToken
    {
        readonly SourceSyntax _value;
        public SyntaxBoxToken(SourceSyntax value) { _value = value; }

        protected override Syntax Terminal(SourcePart token) => _value.Syntax;
        public override string Id => "<box>";
    }
}