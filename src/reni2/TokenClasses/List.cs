using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    sealed class List : TokenClass
    {
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            var leftList = left.ToList(this);
            var rightList = right.ToList(this);
            var compileSyntaxs = leftList.Concat(rightList).ToArray();
            return Container.Create(token, compileSyntaxs);
        }
    }
}