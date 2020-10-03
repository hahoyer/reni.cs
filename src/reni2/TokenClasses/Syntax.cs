using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class Syntax
        : DumpableObject
            , ISyntax
            , ValueCache.IContainer
            , IBinaryTree<Syntax>
    {
        class NullScope
            : DumpableObject
                , IValuesScope
                , IDefaultScopeProvider
        {
            bool IDefaultScopeProvider.MeansPublic => false;
            IDefaultScopeProvider IValuesScope.DefaultScopeProvider => this;
            bool IValuesScope.IsDeclarationPart => false;
        }

        public const bool ValuePropertyIsObsolete = true;
        static int NextObjectId;

        static readonly IValuesScope NullScopeInstance = new NullScope();

        [EnableDump]
        [EnableDumpExcept(null)]
        internal Syntax Left { get; }

        [DisableDump]
        internal SyntaxOption Option { get; }

        [EnableDump]
        [EnableDumpExcept(null)]
        internal Syntax Right { get; }

        [DisableDump]
        internal readonly IToken Token;

        internal ITokenClass TokenClass { get; }

        Syntax
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right
        )
            : base(NextObjectId++)
        {
            Token = token;
            Left = left;
            TokenClass = tokenClass;
            Right = right;

            Option = new SyntaxOption(this);
        }

        [DisableDump]
        internal Result<Declarer> Declarer
        {
            get
            {
                switch(TokenClass)
                {
                    case IDeclarerTokenClass tokenClass:
                        return tokenClass.Get(this);
                    case RightParenthesis _:
                    case EndOfText _:
                    case List _:
                    case ScannerSyntaxError _:
                        return null;
                }

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal SourcePart SourcePart => Option.SourcePart;

        Syntax IBinaryTree<Syntax>.Left => Left;
        Syntax IBinaryTree<Syntax>.Right => Right;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        public bool IsEqual(Syntax other, IComparator differenceHandler)
        {
            if(TokenClass.Id != other.TokenClass.Id)
                return false;

            if(Left == null && other.Left != null)
                return false;

            if(Left != null && other.Left == null)
                return false;

            if(Right == null && other.Right != null)
                return false;

            if(Right != null && other.Right == null)
                return false;

            if(Left != null && !Left.IsEqual(other.Left, differenceHandler))
                return false;

            if(Right != null && !Right.IsEqual(other.Right, differenceHandler))
                return false;

            return CompareWhiteSpaces(Token.PrecededWith, other.Token.PrecededWith, differenceHandler);
        }

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id})";

        internal Result<Value> Value(IValuesScope scope) => this.CachedFunction(scope ?? NullScopeInstance, GetValue);

        internal static Syntax Create
        (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right
        )
            => new Syntax(left, tokenClass, token, right);

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


        internal Result<Statement[]> GetStatements(IValuesScope scope, List type = null)
        {
            var statements = GetStatementsOrDefault(scope, type);
            if(statements != null)
                return statements;

            var statement = GetStatementOrDefault(scope);
            if(statement != null)
                return statement.Convert(x => new[] {x});

            var value = GetValueOrDefault(scope);
            if(value != null)
                return Statement.CreateStatements(value, scope.DefaultScopeProvider);

            return new Result<Statement[]>(new Statement[0], IssueId.InvalidListOperandSequence.Issue(SourcePart));
        }

        internal Result<Value> GetValue(IValuesScope scopeKey)
        {
            var scope = scopeKey == NullScopeInstance? null : scopeKey;

            var value = GetValueOrDefault(scope);
            if(value != null)
                return value;

            var statement = GetStatementOrDefault(scopeKey);
            if(statement != null)
                return CompoundSyntax.Create(statement, this);

            var statements = GetStatementsOrDefault(scope, null);
            if(statements != null)
                return CompoundSyntax.Create(statements, this);

            return IssueId.InvalidExpression.Value(this);
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
                return new Result<Syntax>(Right, IssueId.ExtraLeftBracket.Issue(SourcePart));

            NotImplementedMethod(level, parent);
            return null;
        }

        Result<Value> GetValueOrDefault(IValuesScope scope)
            => scope != null && scope.IsDeclarationPart
                ? null
                : (TokenClass as IValueProvider)?.Get(this, scope);

        Result<Statement> GetStatementOrDefault(IValuesScope scope)
            => (TokenClass as IStatementProvider)?.Get(Left, Right, scope);

        Result<Statement[]> GetStatementsOrDefault(IValuesScope scope, List type)
            => (TokenClass as IStatementsProvider)?.Get(type, this, scope);

        Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;


        Syntax RootOfBelongings(Syntax recent)
        {
            if(!(recent.TokenClass is IBelongingsMatcher matcher))
                return null;

            var sourceSyntaxList = BackChain(recent)
                .ToArray();

            return sourceSyntaxList
                       .Skip(1)
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

        static bool CompareWhiteSpaces
            (IEnumerable<IItem> target, IEnumerable<IItem> other, IComparator differenceHandler)
        {
            if(target.Where(item => item.IsComment()).SequenceEqual(other.Where(item => item.IsComment())
                , differenceHandler.WhiteSpaceComparer))
                return true;

            NotImplementedFunction(target.Dump(), other.Dump(), differenceHandler);
            return default;
        }
    }
}