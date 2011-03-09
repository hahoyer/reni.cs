using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.ReniParser.TokenClasses;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.ReniParser
{
    [Serializable]
    internal abstract class ParsedSyntax : Parser.ParsedSyntax
    {
        protected ParsedSyntax(TokenData token):base(token) { }
        protected ParsedSyntax(TokenData token, int nextObjectId): base(token,nextObjectId) { }

        internal virtual ICompileSyntax ToCompiledSyntax()
        {
            NotImplementedMethod(); //Probably it's a missing right parenthesis
            return null;
        }

        internal virtual ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal virtual ParsedSyntax CreateThenSyntax(TokenData token, ICompileSyntax condition) { return new ThenSyntax(condition, token, ToCompiledSyntax()); }

        internal virtual ParsedSyntax CreateElseSyntax(TokenData token, ICompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        internal virtual ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal virtual ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken)
        {
            NotImplementedMethod(leftToken, rightToken); //Probably it's a missing right parenthesis
            return null;
        }

        internal virtual ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ParsedSyntax right)
        {
            return new ExpressionSyntax(tokenClass, ToCompiledSyntax(), token, right.ToCompiledSyntaxOrNull());
        }

    }

    internal static class ParsedSyntaxExtender
    {
        internal static ICompileSyntax CheckedToCompiledSyntax(this ParsedSyntax parsedSyntax)
        {
            parsedSyntax.AssertIsNotNull();
            return parsedSyntax.ToCompiledSyntax();
        }

        internal static void AssertIsNull(this ParsedSyntax parsedSyntax) { Tracer.Assert(parsedSyntax == null); }

        internal static void AssertIsNotNull(this ParsedSyntax parsedSyntax) { Tracer.Assert(parsedSyntax != null); }

        internal static ICompileSyntax ToCompiledSyntaxOrNull(this ParsedSyntax parsedSyntax)
        {
            if(parsedSyntax == null)
                return null;
            return parsedSyntax.CheckedToCompiledSyntax();
        }
    }
}