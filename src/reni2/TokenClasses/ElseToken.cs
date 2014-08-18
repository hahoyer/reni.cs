using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class ElseToken : TokenClass
    {
        protected override ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            return left
                .CreateElseSyntax(token, right.ToCompiledSyntax());
        }
    }
}