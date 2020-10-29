using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, ITree<BinaryTree>
    {
        internal class BracketNodes
        {
            internal BinaryTree Center;
            internal BinaryTree Left;
            internal BinaryTree Right;

            [DisableDump]
            internal Anchor ToAnchor => Anchor.Create(Left, Right);
        }

        static int NextObjectId;

        [EnableDump(Order = 1)]
        [EnableDumpExcept(null)]
        internal BinaryTree Left { get; }

        [EnableDump(Order = 3)]
        [EnableDumpExcept(null)]
        internal BinaryTree Right { get; }

        [DisableDump]
        internal readonly IToken Token;

        [EnableDump(Order = 2)]
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
        internal IEnumerable<Issue> Issues
            => T(Left?.Issues, T((TokenClass as IErrorToken)?.IssueId.Issue(Token.Characters)), Right?.Issues)
                .ConcatMany()
                .Where(node => node != null);

        [DisableDump]
        internal BracketNodes BracketKernel
        {
            get
            {
                if(!(TokenClass is IRightBracket rightParenthesis))
                    return null;

                var level = rightParenthesis.Level;

                (Right == null).Assert();

                var result = new BracketNodes {Left = Left, Center = Left.Right, Right = this};

                if(!(Left.TokenClass is ILeftBracket leftParenthesis))
                {
                    result.Center = Left;
                    result.Left = ErrorToken.Create(IssueId.MissingLeftBracket, Left.LeftMost);
                    return result;
                }

                (Left.Left == null).Assert();

                var levelDelta = leftParenthesis.Level - level;

                if(levelDelta == 0)
                    return result;

                if(levelDelta > 0)
                {
                    result.Right = ErrorToken.Create(IssueId.MissingRightBracket, RightMost);
                    return result;
                }

                Left.NotImplementedMethod(level, this);
                return null;
            }
        }

        public BinaryTree[] ParserLevelGroup => GetParserLevelGroup(TokenClass).ToArray();

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;
        int ITree<BinaryTree>.DirectChildCount => 2;

        BinaryTree ITree<BinaryTree>.GetDirectChild(int index)
            => index switch
            {
                0 => Left, 1 => Right, _ => null
            };

        int ITree<BinaryTree>.LeftDirectChildCount => 1;

        IEnumerable<BinaryTree> GetParserLevelGroup(ITokenClass tokenClass)
        {
            if(Left != null)
                foreach(var node in Left.GetParserLevelGroup(tokenClass))
                    yield return node;

            if(tokenClass.IsBelongingTo(TokenClass))
                yield return this;

            if(Right != null)
                foreach(var node in Right.GetParserLevelGroup(tokenClass))
                    yield return node;
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

        [Obsolete("", true)]
        internal IEnumerable<BinaryTree> ItemsAsLongAs(Func<BinaryTree, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));

        internal int? GetBracketLevel()
        {
            if(!(TokenClass is IRightBracket rightParenthesis))
                return null;
            var leftParenthesis = Left?.TokenClass as ILeftBracket;
            return T(leftParenthesis?.Level ?? 0, rightParenthesis.Level).Max();
        }
    }
}