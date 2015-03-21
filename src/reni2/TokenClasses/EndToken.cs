using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class EndToken : TokenClass
    {
        protected override Syntax Suffix(Syntax left, IToken token) 
            => left.ToCompiledSyntax.End;
        protected override Syntax Terminal(IToken token) => new EmptyList();
        public override string Id => PrioTable.EndOfText;
    }
}