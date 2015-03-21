using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class EndToken : NonPrefixToken
    {
        protected override Syntax Suffix(Syntax left, SourcePart token) 
            => left.ToCompiledSyntax.End;
        protected override Syntax Terminal(SourcePart token) => new EmptyList();
        public override string Id => PrioTable.EndOfText;
    }
}