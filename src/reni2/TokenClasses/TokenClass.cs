using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using hw.PrioParser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base class for compiler tokens
    /// </summary>
    abstract class TokenClass : hw.Parser.TokenClass, IOperator<ParsedSyntax>
    {
        protected override sealed IParsedSyntax Create(IParsedSyntax left, IPart token, IParsedSyntax right)
        {
            StartMethodDump(false, left, token, right);
            try
            {
                BreakExecution();
                var result = Create((ParsedSyntax) left, (TokenData) token, (ParsedSyntax) right);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal ParsedSyntax Create(ParsedSyntax left, TokenData tokenData, ParsedSyntax right)
        {
            return this.Operation(left, tokenData, right);
        }

        ParsedSyntax IOperator<ParsedSyntax>.Terminal(IOperatorPart token) { return TerminalSyntax((TokenData) token); }
        ParsedSyntax IOperator<ParsedSyntax>.Prefix(IOperatorPart token, ParsedSyntax right)
        {
            return PrefixSyntax((TokenData) token, right);
        }
        ParsedSyntax IOperator<ParsedSyntax>.Suffix(ParsedSyntax left, IOperatorPart token)
        {
            return SuffixSyntax(left, (TokenData) token);
        }
        ParsedSyntax IOperator<ParsedSyntax>.Infix(ParsedSyntax left, IOperatorPart token, ParsedSyntax right)
        {
            return InfixSyntax(left, (TokenData) token, right);
        }

        protected virtual ParsedSyntax TerminalSyntax(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}