using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal abstract class TokenClass : Parser.TokenClass
    {
        sealed protected override IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax((ParsedSyntax) left, token, (ParsedSyntax) right); }

        protected virtual ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string NodeDump { get { return GetType().FullName + " " + Name; } }
    }
}