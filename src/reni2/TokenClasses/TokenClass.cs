using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class TokenClass : TokenClass<Syntax>, IOperator<Syntax>
    {
        protected override sealed Syntax Create(Syntax left, SourcePart token, Syntax right)
        {
            StartMethodDump(false, left, token, right);
            try
            {
                BreakExecution();
                var result = CreateForVisit(left, token, right);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
        internal Syntax CreateForVisit(Syntax left, SourcePart token, Syntax right) { return this.Operation(left, token, right); }


        Syntax IOperator<Syntax>.Terminal(SourcePart token) { return TerminalSyntax(token); }
        Syntax IOperator<Syntax>.Prefix(SourcePart token, Syntax right) { return PrefixSyntax(token, right); }
        Syntax IOperator<Syntax>.Suffix(Syntax left, SourcePart token) { return SuffixSyntax(left, token); }
        Syntax IOperator<Syntax>.Infix(Syntax left, SourcePart token, Syntax right) { return InfixSyntax(left, token, right); }

        protected virtual Syntax TerminalSyntax(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual Syntax PrefixSyntax(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual Syntax SuffixSyntax(Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}