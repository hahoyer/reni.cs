using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class ThenToken : TokenClass
    {
        protected override Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            return right.CreateThenSyntax(token, left.ToCompiledSyntax());
        }
    }
}