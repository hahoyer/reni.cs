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
        protected Syntax(SourcePart token, SourcePart additionalSourcePart = null)
            : base(token, additionalSourcePart) {}

        protected Syntax(SourcePart token, int nextObjectId, SourcePart additionalSourcePart = null)
            : base(token, nextObjectId, additionalSourcePart) {}

        [DisableDump]
        internal virtual CompileSyntax ContainerStatementToCompileSyntax => ToCompiledSyntax;

        [DisableDump]
        internal virtual CompileSyntax ToCompiledSyntax
            => new CompileSyntaxError(IssueId.MissingRightBracket, Token, sourcePart: SourcePart);

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }
        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal virtual Syntax RightParenthesis(int level, SourcePart token)
        {
            NotImplementedMethod(level, token);
            return null;
        }

        internal Syntax CreateThenSyntax(SourcePart token, CompileSyntax condition)
            => new CondSyntax(condition, token, ToCompiledSyntax);

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

        static bool _isInDump;

        protected override sealed string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = CompoundSyntax.IsInContainerDump;
            CompoundSyntax.IsInContainerDump = false;
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
            CompoundSyntax.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        internal virtual IEnumerable<Syntax> ToList(List type) { yield return this; }
        internal virtual CompoundSyntax ToContainer => ListSyntax.Spread(this).ToContainer;
        internal virtual bool IsMutableSyntax => false;
        internal virtual bool IsConverterSyntax => false;
        internal virtual bool IsIdentifier => false;
        internal virtual bool IsText => false;
        internal virtual bool IsKeyword => false;
        internal virtual bool IsNumber => false;
        internal virtual bool IsError => false;

        internal virtual Syntax SyntaxError
            (IssueId issue, SourcePart token, Syntax right = null, SourcePart sourcePart = null)
        {
            NotImplementedMethod(sourcePart, issue, token, right);
            return null;
        }

        public virtual Syntax Sourround(SourcePart sourcePart)
        {
            NotImplementedMethod(sourcePart);
            return null;
        }

        internal virtual Syntax SuffixedBy(Definable definable, SourcePart token)
            => new ExpressionSyntax(definable, ToCompiledSyntax, token, null);

        internal Token LocateToken(SourcePosn sourcePosn)
        {
            if(SourcePart.Contains(sourcePosn))
            {
                var child =
                    Children.Select(item => ((Syntax) item)?.LocateToken(sourcePosn))
                        .FirstOrDefault(item => item != null);
                if(child != null)
                    return child;
                if(Token.Contains(sourcePosn))
                    return new SyntaxToken(this);
            }
            return null;
        }
    }
}