using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class ThenToken : TokenClass
    {
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => right.CreateThenSyntax(token, left.ToCompiledSyntax);
    }
}