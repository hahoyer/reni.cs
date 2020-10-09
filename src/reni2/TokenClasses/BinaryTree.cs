using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, IBinaryTree<BinaryTree>
    {
        interface IVisitor
        {
            void VisitMain(BinaryTree target);
        }

        class IssuesVisitor : DumpableObject, IVisitor
        {
            readonly List<Issue> ValueCollector = new List<Issue>();

            [DisableDump]
            internal IEnumerable<Issue> Value => ValueCollector;

            void IVisitor.VisitMain(BinaryTree target)
            {
                var issue = (target.TokenClass as ScannerSyntaxError)
                    ?.IssueId
                    .Issue(target.Token.Characters);

                if(issue != null)
                    ValueCollector.Add(issue);
            }
        }

        static int NextObjectId;

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



        [DisableDump]
        internal SourcePart SourcePart =>
            LeftMost.Token.SourcePart().Start.Span(RightMost.Token.Characters.End);

        BinaryTree LeftMost => Left?.LeftMost ?? this;
        BinaryTree RightMost => Right?.RightMost ?? this;

        [DisableDump]
        internal IEnumerable<BinaryTree> Items => this.CachedValue(GetItems);

        [DisableDump]
        internal IEnumerable<Issue> Issues
        {
            get
            {
                var visitor = new IssuesVisitor();
                Visit(visitor);
                return visitor.Value;
            }
        }

        BinaryTree IBinaryTree<BinaryTree>.Left => Left;
        BinaryTree IBinaryTree<BinaryTree>.Right => Right;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        void Visit(IVisitor visitor)
        {
            Left?.Visit(visitor);
            visitor.VisitMain(this);
            Right?.Visit(visitor);
        }

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

        Result<BinaryTree> GetBracketKernel(int level)
        {
            var rightParenthesis = TokenClass as IRightBracket;
            Tracer.Assert(rightParenthesis != null);
            Tracer.Assert(level == rightParenthesis.Level);

            Tracer.Assert(Right == null);

            if(!(Left.TokenClass is ILeftBracket leftParenthesis))
                return new Result<BinaryTree>(Left, IssueId.ExtraRightBracket.Issue(SourcePart));

            Tracer.Assert(Left.Left == null);

            var levelDelta = leftParenthesis.Level - level;

            if(levelDelta == 0)
                return Left.Right;

            if(levelDelta > 0)
                return new Result<BinaryTree>(Left.Right, IssueId.ExtraLeftBracket.Issue(Left.SourcePart));

            Left.NotImplementedMethod(level, this);
            return null;
        }

        internal Result<BinaryTree> GetBracketKernel()
            => GetBracketKernel(((IRightBracket)TokenClass).Level);
    }
}