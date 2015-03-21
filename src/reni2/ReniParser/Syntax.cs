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
using Reni.Validation;

namespace Reni.ReniParser
{
    abstract class Syntax : DumpableObject
    {
        protected Syntax() { }

        protected Syntax(int objectId)
            : base(objectId) {}

        [DisableDump]
        internal virtual CompileSyntax ContainerStatementToCompileSyntax => ToCompiledSyntax;

        [DisableDump]
        internal abstract CompileSyntax ToCompiledSyntax { get; }

        internal virtual IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield break;
        }
        internal virtual IEnumerable<string> GetDeclarations() { yield break; }

        internal Syntax CreateThenSyntax(CompileSyntax condition)
            => new CondSyntax(condition, ToCompiledSyntax);

        internal virtual Syntax CreateElseSyntax(CompileSyntax elseSyntax)
        {
            NotImplementedMethod(elseSyntax);
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
            if(!ParsedSyntax.IsDetailedDumpRequired)
                return result;
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

        internal virtual Syntax SuffixedBy(Definable definable, SourcePart token)
            => new ExpressionSyntax(ToCompiledSyntax, definable, null, token);

        [DisableDump]
        internal virtual IEnumerable<Issue> Issues
            => Parts.SelectMany(item => item.DirectIssues).ToArray();

        [DisableDump]
        internal virtual IEnumerable<Issue> DirectIssues { get { yield break; } }


        internal virtual Syntax Match(int level, SourcePart token)
            => new CompileSyntaxError(IssueId.ExtraRightBracket, token);

        [DisableDump]
        internal IEnumerable<Syntax> Parts => DirectChildren
            .Where(item => item != null)
            .SelectMany(item => item.Parts)
            .plus(this);

        [DisableDump]
        protected virtual IEnumerable<Syntax> DirectChildren { get { yield break; } }

        internal ListSyntax ToListSyntax => new ListSyntax
            (
            null,
            ToList(null)
            );
    }


    static class Extension
    {
        internal static T[] plus<T>(this T x, params IEnumerable<T>[] y)
            => new[] {x}.Concat(y.SelectMany(item => item)).ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, IEnumerable<T> y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, T y)
            where T : class
            => (x ?? new T[0])
                .Concat(y.NullableToArray())
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
            (this Syntax target, Func<IssueId> getError, SourcePart source)
            => target?.ToCompiledSyntax
                ?? new CompileSyntaxError(getError(), source);

        internal static Token<SourceSyntax> Token(this SourcePosn sourcePosn)
            => new Token<SourceSyntax>(null, sourcePosn.Span(0));

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