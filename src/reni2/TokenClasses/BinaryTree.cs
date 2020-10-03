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
    public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, IBinaryTree<BinaryTree>
    {
        class NullScope : DumpableObject, IValuesScope, IDefaultScopeProvider
        {
            bool IDefaultScopeProvider.MeansPublic => false;
            IDefaultScopeProvider IValuesScope.DefaultScopeProvider => this;
            bool IValuesScope.IsDeclarationPart => false;
        }

        static int NextObjectId;

        static readonly IValuesScope NullScopeInstance = new NullScope();

        [EnableDump]
        [EnableDumpExcept(null)]
        internal BinaryTree Left { get; }

        [EnableDump]
        [EnableDumpExcept(null)]
        internal BinaryTree Right { get; }

        [DisableDump]
        internal readonly IToken Token;

        internal ITokenClass TokenClass { get; }

        BinaryTree
        (
            BinaryTree left,
            ITokenClass tokenClass,
            IToken token,
            BinaryTree right
        )
            : base(NextObjectId++)
        {
            Token = token;
            Left = left;
            TokenClass = tokenClass;
            Right = right;
        }

        [EnableDumpExcept(null)]
        internal IDeclarationTag DeclarationTag => TokenClass as IDeclarationTag;

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
        internal SourcePart SourcePart =>
            LeftMost.Token.SourcePart().Start.Span(RightMost.Token.Characters.End);

        BinaryTree LeftMost => Left?.LeftMost ?? this;
        BinaryTree RightMost => Right?.RightMost ?? this;

        [DisableDump]
        internal IEnumerable<BinaryTree> Items => this.CachedValue(GetItems);

        BinaryTree IBinaryTree<BinaryTree>.Left => Left;
        BinaryTree IBinaryTree<BinaryTree>.Right => Right;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        public bool IsEqual(BinaryTree other, IComparator differenceHandler)
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

        internal Result<Syntax> Syntax(IValuesScope scope) => this.CachedFunction(scope ?? NullScopeInstance, GetValue);

        internal static BinaryTree Create
        (
            BinaryTree left,
            ITokenClass tokenClass,
            IToken token,
            BinaryTree right
        )
            => new BinaryTree(left, tokenClass, token, right);

        internal IEnumerable<BinaryTree> Belongings(BinaryTree recent)
        {
            var root = RootOfBelongings(recent);

            return root?.TokenClass is IBelongingsMatcher matcher
                ? root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray()
                : null;
        }

        internal IEnumerable<BinaryTree> ItemsAsLongAs(Func<BinaryTree, bool> condition)
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

        internal Result<Syntax> GetValue(IValuesScope scopeKey)
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

        internal Result<BinaryTree> GetBracketKernel(int level, BinaryTree parent)
        {
            Tracer.Assert(parent.Right == null);

            if(!(TokenClass is LeftParenthesis leftParenthesis))
                return new Result<BinaryTree>(this, IssueId.ExtraRightBracket.Issue(parent.SourcePart));

            Tracer.Assert(Left == null);

            var levelDelta = leftParenthesis.Level - level;

            if(levelDelta == 0)
                return Right;

            if(levelDelta > 0)
                return new Result<BinaryTree>(Right, IssueId.ExtraLeftBracket.Issue(SourcePart));

            NotImplementedMethod(level, parent);
            return null;
        }

        Result<Syntax> GetValueOrDefault(IValuesScope scope)
            => scope != null && scope.IsDeclarationPart
                ? null
                : (TokenClass as IValueProvider)?.Get(this, scope);

        Result<Statement> GetStatementOrDefault(IValuesScope scope)
            => (TokenClass as IStatementProvider)?.Get(Left, Right, scope);

        Result<Statement[]> GetStatementsOrDefault(IValuesScope scope, List type)
            => (TokenClass as IStatementsProvider)?.Get(type, this, scope);

        BinaryTree Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        BinaryTree CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;


        BinaryTree RootOfBelongings(BinaryTree recent)
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

        IEnumerable<BinaryTree> BackChain(BinaryTree recent)
        {
            var subChain = SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return this;
        }

        BinaryTree[] SubBackChain(BinaryTree recent)
        {
            if(this == recent)
                return new BinaryTree[0];

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

        IEnumerable<BinaryTree> GetItems()
        {
            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;

            yield return this;

            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }
    }
}