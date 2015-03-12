using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TokenClass
    {
        readonly ReniParser.Syntax _value;
        public SyntaxBoxToken(ReniParser.Syntax value) { _value = value; }

        protected override ReniParser.Syntax Terminal(IToken token) => _value;

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(IToken token)
                : base(token) { }
        }

        public override string Id => "<box>";
    }
}