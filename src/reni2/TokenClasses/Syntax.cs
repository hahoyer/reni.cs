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
    public sealed class Syntax : DumpableObject, ISourcePartProxy, ValueCache.IContainer, ISyntax
    {
        static int NextObjectId;

        internal static Syntax CreateSourceSyntax
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            => new Syntax(left, tokenClass, token, right);

        Syntax _parent;

        Syntax
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            : base(NextObjectId++)
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

        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        SourcePart ISourcePartProxy.All => SourcePart;
        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        [DisableDump]
        internal SyntaxOption Option {get;}

        internal Syntax Left {get;}
        internal ITokenClass TokenClass {get;}

        [DisableDump]
        internal IToken Token {get;}

        internal Syntax Right {get;}

        FunctionCache<int, Syntax> LocatePositionCache {get;}

        [DisableDump]
        internal Syntax Parent
        {
            get => _parent;
            private set
            {
                Tracer.Assert(_parent == null);
                _parent = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart() + Right?.SourcePart;

        [DisableDump]
        internal Syntax LeftNeighbor => Left?.RightMost ?? LeftParent;

        [DisableDump]
        internal Syntax RightNeighbor => Right?.LeftMost ?? RightParent;

        [DisableDump]
        internal Syntax LeftParent => Parent?.Left == this ? Parent.LeftParent : Parent;

        [DisableDump]
        internal Syntax RightParent => Parent?.Right == this ? Parent.RightParent : Parent;


        [DisableDump]
        internal Syntax LeftMost => Left?.LeftMost ?? this;

        [DisableDump]
        internal Syntax RightMost => Right?.RightMost ?? this;

        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

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
            => Option
                .Declarations
                .Distinct()
                .ToArray();

        [DisableDump]
        internal Result<Value> Value => this.CachedValue(() => GetValue(this));


        [DisableDump]
        internal Result<Declarator> Declarer
        {
            get
            {
                switch(TokenClass)
                {
                    case IDeclaratorTokenClass tokenClass: return tokenClass.Get(this);
                    case RightParenthesis _:
                    case EndOfText _:
                    case List _: 
                    case ScannerSyntaxError _: return null;
                }

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal Result<Statement[]> ForceStatements => this.CachedValue(() => GetStatements());

        [DisableDump]
        internal IEnumerable<Issue> Issues => Option.Issues;

        [DisableDump]
        internal IEnumerable<Issue> AllIssues
            => Left?.AllIssues
                .plus(Issues)
                .plus(Right?.AllIssues);

        [DisableDump]
        internal IDefaultScopeProvider DefaultScopeProvider
            => TokenClass as IDefaultScopeProvider ?? Parent?.DefaultScopeProvider;

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

            return root?.TokenClass is IBelongingsMatcher matcher
                ? root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray()
                : null;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));


        internal Syntax RootOfBelongings(Syntax recent)
        {
            if(!(recent.TokenClass is IBelongingsMatcher matcher))
                return null;

            var sourceSyntaxs = BackChain(recent)
                .ToArray();

            return sourceSyntaxs
                       .Skip(count: 1)
                       .TakeWhile(item => matcher.IsBelongingTo(item.TokenClass))
                       .LastOrDefault() ??
                   recent;
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

        IEnumerable<Syntax> GetItems()
        {
            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;

            yield return this;

            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }

        Result<Value> GetValue(Syntax syntax)
        {
            var value = Option.Value;
            if(value != null)
                return value;

            var statement = Option.Statement;
            if(statement != null)
                return CompoundSyntax.Create(statement, syntax);

            var statements = Option.Statements;
            if(statements != null)
                return CompoundSyntax.Create(statements, syntax);

            return IssueId.InvalidExpression.Value(syntax);
        }

        internal Result<Statement[]> GetStatements(List type = null)
        {
            var statements = Option.GetStatements(type);
            if(statements != null)
                return statements;

            var statement = Option.Statement;
            if(statement != null)
                return statement.Convert(x => new[] {x});

            var value = Option.Value;
            if(value != null)
                return Statement.CreateStatements(value, Option.DefaultScopeProvider);

            return new Result<Statement[]>
                (new Statement[0], IssueId.InvalidListOperandSequence.Issue(SourcePart));
        }

        internal Result<Syntax> GetBracketKernel(int level, Syntax parent)
        {
            Tracer.Assert(parent.Right == null);

            if(!(TokenClass is LeftParenthesis leftParenthesis))
                return new Result<Syntax>(this, IssueId.ExtraRightBracket.Issue(parent.SourcePart));

            Tracer.Assert(Left == null);

            var levelDelta = leftParenthesis.Level - level;

            if(levelDelta == 0)
                return Right;

            if(levelDelta > 0)
                return new Result<Syntax>
                    (Right, IssueId.ExtraLeftBracket.Issue(SourcePart));

            NotImplementedMethod(level, parent);
            return null;
        }

        internal IEnumerable<IFormatItem> GetTokenList(SourcePart targetPart, bool isStart)
        {
            var mainTokenPart = Token.SourcePart();
            if(Left != null && targetPart.Position < mainTokenPart.Position)
                foreach(var item in Left.GetTokenList(targetPart, isStart))
                {
                    yield return item;
                    isStart = false;
                }

            var isTokenInRange = Token.Characters.Length > 0 && Token.Characters.Intersect(targetPart) != null;

            if(isStart || !isTokenInRange)
                foreach(var item in Token.PrecededWith)
                    if(item.SourcePart.Intersect(targetPart) != null)
                        yield return new WhiteItem(item, this);

            if(isTokenInRange)
            {
                yield return new TokenItem(this, isStart);
                isStart = false;
            }

            if(Right == null || targetPart.EndPosition <= mainTokenPart.EndPosition)
                yield break;

            foreach(var item in Right.GetTokenList(targetPart, isStart))
                yield return item;
        }

        public bool IsEqual(Syntax other, IComparator diffenceHandler)
        {
            if(TokenClass != other.TokenClass)
                return false;

            NotImplementedMethod(other, diffenceHandler);
            return false;
        }
    }
}