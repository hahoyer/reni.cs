using System;
using System.Collections.Generic;
using hw.Helper;
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
    abstract class Syntax : ParsedSyntax, IPropertyProvider
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

        protected virtual Syntax Surround(PropertyProvider other) => new ProxySyntax(this, other);

        internal Syntax CheckedSurround(IPropertyProvider provider, SourcePart sourcePart)
        {
            if(provider == null || SourceParts.Contain(sourcePart))
                return this;

            return Surround(new PropertyProvider(provider, sourcePart));
        }

        internal new virtual SourcePart SourcePart
            => base.SourcePart +
                DirectChildren
                    .Where(item => item != null)
                    .Select(item => item.SourcePart)
                    .Aggregate();

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
            if(!isInDump && SourcePart.Source.IsPersistent)
                result += FilePosition();
            if(isInContainerDump)
                result += " ObjectId=" + ObjectId;
            else
                result += "\n" + base.Dump(false);
            CompoundSyntax.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        protected override string GetNodeDump() => GetType().PrettyName();

        internal virtual IEnumerable<Syntax> ToList(List type) { yield return this; }
        [DisableDump]
        internal virtual CompoundSyntax ToContainer => ToListSyntax.ToContainer;
        [DisableDump]
        internal virtual bool IsMutableSyntax => false;
        [DisableDump]
        internal virtual bool IsConverterSyntax => false;
        [DisableDump]
        internal virtual bool IsIdentifier => false;
        [DisableDump]
        internal virtual bool IsText => false;
        [DisableDump]
        internal virtual bool IsKeyword => false;
        [DisableDump]
        internal virtual bool IsNumber => false;
        [DisableDump]
        internal virtual bool IsError => false;
        [DisableDump]
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
            => Parts.SelectMany(item => item.DirectIssues).ToArray();

        [DisableDump]
        internal virtual IEnumerable<IssueBase> DirectIssues { get { yield break; } }


        internal TokenInformation LocateToken(SourcePosn sourcePosn)
        {
            var token = Token;
            if(token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            if(!SourcePart.Contains(sourcePosn))
                return null;

            NotImplementedMethod(sourcePosn);

            var whiteSpaceToken = token.PrecededWith.First
                (item => item.Characters.Contains(sourcePosn));
            return new UserInterface.WhiteSpaceToken(whiteSpaceToken);
        }

        internal virtual Syntax RightParenthesis(RightParenthesis.Syntax rightBracket)
            => new CompileSyntaxError(IssueId.ExtraRightBracket, Token);

        [DisableDump]
        internal IEnumerable<Syntax> Parts => DirectChildren
            .Where(item => item != null)
            .SelectMany(item => item.Parts)
            .plus(this);

        [DisableDump]
        protected virtual IEnumerable<Syntax> DirectChildren { get { yield break; } }

        [DisableDump]
        internal SourcePart[] SourceParts => Parts
            .SelectMany(item => item.Token.Parts())
            .Aggregate(new[] {SourcePart}, Extension.Combine)
            .ToArray();

        sealed class ProxySyntax : Syntax
        {
            public ProxySyntax(Syntax value, PropertyProvider other)
                : base(value.Token)
            {
                Value = value;
                Other = other;
            }

            Syntax Value { get; }
            public PropertyProvider Other { get; }

            internal override SourcePart SourcePart => Value.SourcePart + Other.SourcePart.All;
            internal override bool IsConverterSyntax => Value.IsConverterSyntax;
            internal override Syntax UnProxy() => Value.UnProxy();

            internal override CompileSyntax ToCompiledSyntax
                => Value.ToCompiledSyntax.SurroundCompileSyntax(Other);
            internal override Syntax CreateDeclarationSyntax(IToken token, Syntax right)
                => Value.CreateDeclarationSyntax(token, right);
            internal override CompileSyntax ContainerStatementToCompileSyntax
                => Value.ContainerStatementToCompileSyntax;
        }

        internal ListSyntax ToListSyntax => new ListSyntax
            (
            null,
            SourcePart.End.Token(),
            ToList(null)
            );

        internal virtual Syntax UnProxy() => this;
    }

    sealed class PropertyProvider : DumpableObject
    {
        public IPropertyProvider Provider { get; set; }
        public ISourcePart SourcePart { get; set; }
        public PropertyProvider(IPropertyProvider provider, ISourcePart sourcePart)
        {
            Provider = provider;
            SourcePart = sourcePart;
        }
    }

    interface IPropertyProvider {}


    static class Extension
    {
        internal static T[] plus<T>(this T x, params IEnumerable<T>[] y)
            => new[] {x}.Concat(y.SelectMany(item => item)).ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, params T[] y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this T x, params T[] y)
            => new[] {x}.Concat(y).ToDistinctNotNullArray();

        internal static T[] ToDistinctNotNullArray<T>(this IEnumerable<T> y)
            => (y).Where(item => item != null).Distinct().ToArray();

        internal static SourcePart[] Combine(SourcePart[] current, SourcePart next)
            => SourcePart
                .SaveCombine(current.plus(next))
                .ToArray();

        internal static IEnumerable<SourcePart> Parts(this IToken token)
        {
            if(token == null)
                yield break;
            foreach(var whiteSpaceToken in token.PrecededWith)
                yield return whiteSpaceToken.Characters;
            yield return token.Characters;
        }

        internal static CompileSyntax CheckedToCompiledSyntax
            (this Syntax target, IToken token, Func<IssueId> getError)
            => target?.ToCompiledSyntax
                ?? new CompileSyntaxError(getError(), token);

        internal static Token<Syntax> Token(this SourcePosn sourcePosn)
            => new Token<Syntax>(null, sourcePosn.Span(0));

        public static bool Contain
            (this IEnumerable<SourcePart> source, IEnumerable<SourcePart> value)
            => value.All(source.Contain);

        public static bool Contain(this IEnumerable<SourcePart> source, SourcePart value)
        {
            foreach(var sourcePart in source
                .Where(item => item.Source == value.Source)
                .OrderBy(item => item.Position)
                .Where(item => value.Start < item.End)
                .Where(item => item.Start < value.End))
            {
                if(value.Start < sourcePart.Start)
                    return false;

                if(value.Start >= sourcePart.End)
                    continue;

                if(value.End <= sourcePart.End)
                    return true;

                value = sourcePart.End.Span(value.End);
            }

            return false;
        }
    }
}