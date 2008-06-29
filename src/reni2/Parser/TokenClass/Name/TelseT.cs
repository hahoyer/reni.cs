using System;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    internal class TelseT : Infix
    {
        internal override string DumpShort()
        {
            return "else";
        }

        internal override Result Result(ContextBase context, Category category, ICompileSyntax left, Token token, ICompileSyntax right)
        {
            throw new NotImplementedException();
        }
    }
}