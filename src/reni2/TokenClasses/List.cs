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
        protected override Syntax Suffix(Syntax left, SourcePart token)
        {
            return new ListSyntax(this, token, left.ToList(this));
        }
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            return new ListSyntax(this, token, left.ToList(this).Concat(right.ToList(this)).ToArray());
        }
    }
}