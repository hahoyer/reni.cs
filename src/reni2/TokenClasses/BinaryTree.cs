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
    public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, ITree<BinaryTree>
    {
        static int NextObjectId;

        [EnableDump]
        [EnableDumpExcept(null)]
        internal BinaryTree Left { get; }

        [DisableDump]
        internal readonly IToken Token;

        internal ITokenClass TokenClass { get; }

        [EnableDump]
        [EnableDumpExcept(null)]
        internal BinaryTree Right { get; }

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
        internal IEnumerable<Issue> Issues
        {
            get
            {
                if(Left != null)
                    foreach(var issue in Left.Issues)
                        yield return issue;

                var issue1 = (TokenClass as ScannerSyntaxError)?.IssueId.Issue(Token.Characters);
                if(issue1 != null)
                    yield return issue1;

                if(Right != null)
                    foreach(var issue in Right.Issues)
                        yield return issue;
            }
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;

        int ITree<BinaryTree>.LeftDirectChildCount => 1;
        int ITree<BinaryTree>.DirectChildCount => 2;

        BinaryTree ITree<BinaryTree>.GetDirectChild(int index)
            => index switch
            {
                0 => Left
                , 1 => Right
                , _ => null
            };

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id})";

        internal static BinaryTree Create
        (
            BinaryTree left,
            ITokenClass tokenClass,
            IToken token,
            BinaryTree right
        )
            => new BinaryTree(left, tokenClass, token, right);

        [Obsolete("",true)]
        internal IEnumerable<BinaryTree> ItemsAsLongAs(Func<BinaryTree, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));

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

        Result<BinaryTree> GetBracketSubLevel(int level)
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
                return Left;

            if(levelDelta > 0)
                return new Result<BinaryTree>(Left, IssueId.ExtraLeftBracket.Issue(Left.SourcePart));

            Left.NotImplementedMethod(level, this);
            return null;
        }

        internal Result<BinaryTree> GetBracketKernel()
            => GetBracketKernel(((IRightBracket)TokenClass).Level);
        internal Result<BinaryTree> GetBracketSubLevel()
            => GetBracketSubLevel(((IRightBracket)TokenClass).Level);
    }
}