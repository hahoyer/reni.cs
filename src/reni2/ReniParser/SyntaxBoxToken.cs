﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TokenClass
    {
        readonly SourceSyntax _value;
        public SyntaxBoxToken(SourceSyntax value) { _value = value; }

        protected override ReniParser.Syntax Terminal(IToken token) => _value.Syntax;

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string Id => "<box>";
    }
}