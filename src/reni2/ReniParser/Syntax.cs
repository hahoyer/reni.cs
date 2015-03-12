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
        protected Syntax(IToken token)
            : base(token) {}

        protected Syntax(IToken token, int objectId)
            : base(token, objectId) {}

        [DisableDump]
        internal virtual CompileSyntax ContainerStatementToCompileSyntax => ToCompiledSyntax;

        [DisableDump]
        internal virtual CompileSyntax ToCompiledSyntax
            => new CompileSyntaxError(IssueId.CompiledSyntaxExpected, Token);

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

        internal virtual Syntax CreateDeclarationSyntax(IToken token, Syntax right)
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
        [DisableDump]
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
            (IssueId issue, IToken token, Syntax right = null)
        {
            NotImplementedMethod(issue, token, right);
            return null;
        }

        internal virtual Syntax SuffixedBy(DefinableTokenSyntax definable)
            => new ExpressionSyntax(ToCompiledSyntax, definable, null);

        [DisableDump]
        internal IEnumerable<IssueBase> Issues
            => Parts().SelectMany(item => item.DirectIssues).ToArray();

        [DisableDump]
        internal virtual IEnumerable<IssueBase> DirectIssues { get { yield break; } }


        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            var token = Token;
            if(token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            if(!SourcePart.Contains(sourcePosn))
                return null;

            var child =
                Token
                    .OtherParts<Syntax>()
                    .Select(item => item?.LocateToken(sourcePosn))
                    .FirstOrDefault(item => item != null);

            if(child != null)
                return child;

            var whiteSpaceToken = token.PrecededWith.First
                (item => item.Characters.Contains(sourcePosn));
            return new UserInterface.WhiteSpaceToken(whiteSpaceToken);
        }

        internal virtual Syntax RightParenthesis(RightParenthesis.Syntax rightBracket)
            => new CompileSyntaxError(IssueId.ExtraRightBracket, Token);

        internal IEnumerable<Syntax> Parts()
            => new[] {this}
                .Concat
                (DirectChildren().SelectMany(item => item == null ? new Syntax[0] : item.Parts()));

        protected virtual IEnumerable<Syntax> DirectChildren() { yield break; }
    }


    static class Extension
    {
        internal static T[] plus<T>(this T x, params IEnumerable<T>[] y)
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