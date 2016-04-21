using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class Syntax : DumpableObject, ISourcePart, ValueCache.IContainer
    {
        internal static Syntax CreateSourceSyntax
            (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            => new Syntax(left, tokenClass, token, right);

        static int _nextObjectId;

        [DisableDump]
        internal SyntaxOption Option { get; }
        Syntax _parent;
        internal Syntax Left { get; }
        internal ITokenClass TokenClass { get; }
        [DisableDump]
        internal IToken Token { get; }
        internal Syntax Right { get; }

        FunctionCache<int, Syntax> LocatePositionCache { get; }
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        Syntax
            (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            : base(_nextObjectId++)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
            Option = new SyntaxOption(this);
            LocatePositionCache = new FunctionCache<int, Syntax>(LocatePositionForCache);

            if(Left != null)
                Left.Parent = this;

            if(Right != null)
                Right.Parent = this;
        }

        [DisableDump]
        internal Syntax Parent
        {
            get { return _parent; }
            private set
            {
                if(value == null)
                    throw new ArgumentNullException(nameof(value));

                Tracer.Assert(_parent == null);
                _parent = value;
            }
        }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        public Syntax LocatePosition(int current) => LocatePositionCache[current];

        Syntax LocatePositionForCache(int current)
        {
            if(current < SourcePart.Position || current >= SourcePart.EndPosition)
                return Parent?.LocatePosition(current);

            return Left?.CheckedLocatePosition(current) ??
                Right?.CheckedLocatePosition(current) ??
                    this;
        }

        Syntax CheckedLocatePosition(int current)
            =>
                SourcePart.Position <= current && current < SourcePart.EndPosition
                    ? LocatePosition(current)
                    : null;

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
                Right?.CheckedLocate(part) ??
                    this;

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part) ? Locate(part) : null;

        internal IEnumerable<Syntax> Belongings(Syntax recent)
        {
            var root = RootOfBelongings(recent);

            var matcher = root?.TokenClass as IBelongingsMatcher;
            return matcher == null
                ? null
                : root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray();
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));


        internal Syntax RootOfBelongings(Syntax recent)
        {
            var matcher = recent.TokenClass as IBelongingsMatcher;
            if(matcher == null)
                return null;

            var sourceSyntaxs = BackChain(recent)
                .ToArray();

            return sourceSyntaxs
                .Skip(1)
                .TakeWhile(item => matcher.IsBelongingTo(item.TokenClass))
                .LastOrDefault()
                ?? recent;
        }

        IEnumerable<Syntax> BackChain(Syntax recent)
        {
            var subChain = SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return this;
        }

        Syntax[] SubBackChain(Syntax recent)
        {
            if(this == recent)
                return new Syntax[0];

            if(Left != null)
            {
                var result = Left.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            if(Right != null)
            {
                var result = Right.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            return null;
        }

        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        IEnumerable<Syntax> GetItems()
        {
            yield return this;

            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;
            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }

        public string BraceMatchDump => new BraceMatchDumper(this, 3).Dump();

        [DisableDump]
        internal IEnumerable<WhiteSpaceToken> LeadingWhiteSpaceTokens
            => Left != null
                ? Left.LeadingWhiteSpaceTokens
                : Token.PrecededWith.OnlyLeftPart();

        [DisableDump]
        internal ITokenClass LeftMostTokenClass
            => Left == null ? TokenClass : Left.LeftMostTokenClass;

        [DisableDump]
        internal ITokenClass RightMostTokenClass
            => Right == null ? TokenClass : Right.RightMostTokenClass;

        [DisableDump]
        public IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        [DisableDump]
        public string[] DeclarationOptions
        {
            get
            {
                var preType = Option.PreType;

                NotImplementedMethod();
                return null;

                var syntax = Value
                    .Target;
                var functionCache = syntax.ResultCache;
                if(functionCache.Any())
                    return functionCache
                        .Select(item => item.Value.Type)
                        .SelectMany(item => item.DeclarationOptions)
                        .Distinct()
                        .ToArray();

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal Result<Value> Value => this.CachedValue(GetValue);

        Result<Value> GetValue()
        {
            var value = Option.Value;
            if(value != null)
                return value;

            var statement = Option.Statement;
            if (statement != null)
                return CompoundSyntax.Create(statement);

            var statements = Option.Statements;
            if (statements != null)
                return CompoundSyntax.Create(statements);

            return IssueId.InvalidExpression.Value(Token.Characters);
        }


        [DisableDump]
        internal Result<Declarator> Declarator
        {
            get
            {
                var declaratorTokenClass = TokenClass as IDeclaratorTokenClass;
                if(declaratorTokenClass != null)
                    return declaratorTokenClass.Get(Left, Token.Characters, Right);

                NotImplementedMethod();
                return null;
            }
        }

        internal Result<Statement[]> ForceStatements => this.CachedValue(() => GetStatements());

        internal Result<Statement[]> GetStatements(List type = null)
        {
            var statements = Option.GetStatements(type);
            if (statements != null)
                return statements;

            var statement = Option.Statement;
            if (statement != null)
                return statement.Convert(x => new[] { x });

            var value = Option.Value;
            if (value != null)
                return Statement.CreateStatements(Token.Characters, value);

            NotImplementedMethod(type);
            return null;
        }

        [DisableDump]
        internal IEnumerable<Issue> Issues => Option.Issues;

        [DisableDump]
        internal IEnumerable<Issue> AllIssues
            => Left?.AllIssues
                .plus(Issues)
                .plus(Right?.AllIssues);

        internal Tuple<Syntax, Issue> GetBracketKernel
            (int level, SourcePart token, Syntax right = null)
        {
            Tracer.Assert(right == null);
            var leftParenthesis = TokenClass as LeftParenthesis;

            if(leftParenthesis == null)
                return new Tuple<Syntax, Issue>(this, IssueId.ExtraRightBracket.CreateIssue(token));

            Tracer.Assert(Left == null);

            var levelDelta = leftParenthesis.Level - level;

            if(levelDelta == 0)
                return new Tuple<Syntax, Issue>(Right, null);

            if(levelDelta > 0)
                return new Tuple<Syntax, Issue>
                    (Right, IssueId.ExtraLeftBracket.CreateIssue(Token.Characters));

            NotImplementedMethod(level, right);
            return null;
        }

    }

    interface IDeclarationItem
    {
        bool IsDeclarationPart(Syntax syntax);
    }

    interface IValueProvider
    {
        Result<Value> Get(Syntax left, SourcePart token, Syntax right);
    }

    interface IStatementProvider
    {
        Result<Statement> Get(Syntax left, SourcePart token, Syntax right);
    }

    interface IStatementsProvider
    {
        Result<Statement[]> Get(List type, Syntax left, SourcePart token, Syntax right);
    }

    interface IBelongingsMatcher
    {
        bool IsBelongingTo(IBelongingsMatcher otherMatcher);
    }
}