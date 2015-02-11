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
        protected override Syntax Prefix(SourcePart token, Syntax right)
            => ListSyntax(new EmptyList(token), token, right);

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => ListSyntax(left, token, new EmptyList(token));

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => ListSyntax(left, token, right);

        ListSyntax ListSyntax(Syntax left, SourcePart token, Syntax right)
            => new ListSyntax(this, token, left.ToList(this).Concat(right.ToList(this)).ToArray());
    }
}