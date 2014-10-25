using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class List : TokenClass
    {
        protected override Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            return new ListSyntax(left, token, right);
        }
    }
}