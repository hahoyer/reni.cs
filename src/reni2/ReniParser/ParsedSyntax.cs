using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Struct;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    abstract class ParsedSyntax : ParsedSyntaxBase
    {
        protected ParsedSyntax(TokenData token)
            : base(token)
        {}

        protected ParsedSyntax(TokenData token, int nextObjectId)
            : base(token, nextObjectId)
        {}

        internal virtual CompileSyntax ToCompiledSyntax()
        {
            NotImplementedMethod(); //Probably it's a missing right parenthesis
            return null;
        }

        internal virtual ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal virtual ParsedSyntax CreateThenSyntax(TokenData token, CompileSyntax condition)
        {
            return new ThenSyntax(condition, token, ToCompiledSyntax());
        }

        internal virtual ParsedSyntax CreateElseSyntax(TokenData token, CompileSyntax elseSyntax)
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

        internal virtual ParsedSyntax CreateSyntaxOrDeclaration(Definable tokenClass, TokenData token, ParsedSyntax right)
        {
            return new ExpressionSyntax(tokenClass, ToCompiledSyntax(), token, right.ToCompiledSyntaxOrNull());
        }

        static bool _isInDump;

        protected override sealed string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = Container.IsInContainerDump;
            Container.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = GetNodeDump();
            if(!IsDetailedDumpRequired)
                return result;
            if(!isInDump)
                result += FilePosition();
            if(isInContainerDump)
                result += " ObjectId=" + ObjectId;
            else
                result += "\n" + base.Dump(false);
            Container.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        internal CompileSyntax MustBeNullError(Func<IssueId> getIssue)
        {
            NotImplementedMethod(getIssue());
            return null;
        }
    }
}