using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class ElseToken : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            if (left == null)
                return LeftMustNotBeNullError();
            return left.CreateElseSyntax(token, right.CheckedToCompiledSyntax(token, RightMustNotBeNullError));
        }
        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
        CompileSyntaxError LeftMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}