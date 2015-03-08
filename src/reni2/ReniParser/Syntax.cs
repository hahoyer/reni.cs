using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.UserInterface;
using Reni.Validation;

namespace Reni.ReniParser
{
    abstract class Syntax : ParsedSyntax
    {
        readonly ParsedSyntax[] _parts;

        protected Syntax(Token token)
            : base(token) {}

        protected Syntax(Token token, int objectId)
            : base(token, objectId) {}

        protected Syntax(Syntax other, params ParsedSyntax[] parts)
            : this(other.Token)
        {
            _parts = other._parts.plus(parts);
        }

        protected Syntax(Syntax other, int objectId, params ParsedSyntax[] parts)
            : this(other.Token, objectId)
        {
            _parts = other._parts.plus(parts);
        }


        [DisableDump]
        internal virtual CompileSyntax ContainerStatementToCompileSyntax => ToCompiledSyntax;

        [DisableDump]
        internal virtual CompileSyntax ToCompiledSyntax
            => new CompileSyntaxError(IssueId.MissingRightBracket, Token)
                .SurroundCompileSyntax(_parts);

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }
        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal Syntax CreateThenSyntax(ThenToken.Syntax thenToken, CompileSyntax condition)
            => new CondSyntax(condition, thenToken, ToCompiledSyntax);

        internal virtual Syntax CreateElseSyntax(ElseToken.Syntax token, CompileSyntax elseSyntax)
        {
            NotImplementedMethod(token, elseSyntax);
            return null;
        }

        internal virtual Syntax CreateDeclarationSyntax(Token token, Syntax right)
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
        internal virtual bool IsBraceLike => false;

        internal virtual Syntax SyntaxError
            (
            IssueId issue,
            Token token,
            Syntax right = null,
            params ParsedSyntax[] parts)
        {
            NotImplementedMethod(issue, token, right, (object) parts);
            return null;
        }

        internal virtual Syntax Surround(params ParsedSyntax[] parts)
        {
            NotImplementedMethod((object) parts);
            return null;
        }

        internal virtual Syntax SuffixedBy(Definable definable, Token token)
            => new ExpressionSyntax(definable, ToCompiledSyntax, token, null);

        protected override ParsedSyntax[] Children
            => _parts.plus(SyntaxChildren.ToArray<ParsedSyntax>());

        protected virtual IEnumerable<Syntax> SyntaxChildren { get { yield break; } }

        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            if(!SourcePart.Contains(sourcePosn))
                return null;

            var child =
                Children.Select(item => ((Syntax) item)?.LocateToken(sourcePosn))
                    .FirstOrDefault(item => item != null);

            if(child != null)
                return child;

            if(Token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            var whiteSpaceToken = Token.PreceededBy.First
                (item => item.Characters.Contains(sourcePosn));
            return new UserInterface.WhiteSpaceToken(whiteSpaceToken);
        }
    }


    static class Extension
    {
        internal static T[] plus<T>(this T x, params T[][] y)
            => new[] {x}.Concat(y.SelectMany(item => item)).ToDisitncNotNullArray();

        internal static T[] plus<T>(this T[] x, params T[] y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDisitncNotNullArray();

        internal static T[] plus<T>(this T x, params T[] y)
            => new[] {x}.Concat(y).ToDisitncNotNullArray();

        internal static T[] ToDisitncNotNullArray<T>(this IEnumerable<T> y)
            => (y).Where(item => item != null).Distinct().ToArray();
    }
}