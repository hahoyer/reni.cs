using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class EndToken : TokenClass
    {
        protected override Syntax Suffix(Syntax left, SourcePart token) => left.ToCompiledSyntax;
    }
}