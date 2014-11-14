using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    abstract class Syntax : ParsedSyntax
    {
        protected Syntax(SourcePart token)
            : base(token)
        {}

        protected Syntax(SourcePart token, int nextObjectId)
            : base(token, nextObjectId)
        {}

        [DisableDump]
        internal virtual Syntax ExtractBody { get { return this; } }

        [DisableDump]
        internal virtual CompileSyntax ToCompiledSyntax
        {
            get
            {
                NotImplementedMethod(); //Probably it's a missing right parenthesis
                return null;
            }
        }
        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index) { yield break; }

        internal virtual Syntax RightParenthesis(int level, SourcePart token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal virtual Syntax RightParenthesisOnLeft(int level, SourcePart token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal virtual Syntax RightParenthesisOnRight(int level, SourcePart token)
        {
            return SurroundedByParenthesis(Token, token);
        }

        internal virtual Syntax CreateThenSyntax(SourcePart token, CompileSyntax condition)
        {
            return new ThenSyntax(condition, token, ToCompiledSyntax);
        }

        internal virtual Syntax CreateElseSyntax(SourcePart token, CompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        internal virtual Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        internal virtual Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken)
        {
            NotImplementedMethod(leftToken, rightToken); //Probably it's a missing right parenthesis
            return null;
        }

        internal Syntax CreateSyntaxOrDeclaration(Definable tokenClass, SourcePart token, Syntax right)
        {
            return new ExpressionSyntax(tokenClass, ToCompiledSyntax, token, right.ToCompiledSyntaxOrNull());
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

        internal virtual IEnumerable<Syntax> ToList(List type) { yield return this; }
        internal virtual Container ToContainer { get { return ListSyntax.Spread(this).ToContainer; } }
    }
}