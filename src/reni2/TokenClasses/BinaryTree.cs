using System.Collections.Generic;
using System.Globalization;
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
            internal Anchor ToAnchor
            {
                get { return Anchor.Create(Left, Right); }
            }
        }

        static int NextObjectId;

        [EnableDump(Order = 1)]
        [EnableDumpExcept(null)]
        internal BinaryTree Left { get; }

        [DisableDump]
        internal BinaryTree LeftNeighbor;

        [DisableDump]
        internal BinaryTree Parent;

        [EnableDump(Order = 3)]
        [EnableDumpExcept(null)]
        internal BinaryTree Right { get; }

        [DisableDump]
        internal BinaryTree RightNeighbor;

        [DisableDump]
        internal readonly IToken Token;

        [DisableDump]
        internal ITokenClass TokenClass { get; }

        int Depth;

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

            SetLinks();
        }


        [DisableDump]
        internal SourcePart SourcePart =>
            LeftMost.Token.SourcePart().Start.Span(RightMost.Token.Characters.End);

        [DisableDump]
        internal BinaryTree LeftMost => Left?.LeftMost ?? this;
        [DisableDump]
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

        [DisableDump]
        public BinaryTree[] ParserLevelGroup => this.CachedValue(()=>GetParserLevelGroup(TokenClass).ToArray());

        [DisableDump]
        public bool IsSeparatorRequired
            => !Token.PrecededWith.HasComment() && SeparatorExtension.Get(LeftNeighbor?.TokenClass, TokenClass);

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

        void SetLinks()
        {
            if(Left != null)
            {
                Left.Parent = this;
                Left.Depth = Depth + 1;
                Left.Chain(node => node.Right).Last().RightNeighbor = this;
            }

            if(Right != null)
            {
                Right.Parent = this;
                Right.Depth = Depth + 1;
                Right.Chain(node => node.Left).Last().LeftNeighbor = this;
            }
        }

        IEnumerable<BinaryTree> GetParserLevelGroup(ITokenClass tokenClass)
        {
            if(tokenClass is not IBelongingsMatcher)
                return new BinaryTree[0];
            
            if(tokenClass is List)
                return this
                    .Chain(node => tokenClass.IsBelongingTo(node.Right?.TokenClass)? node.Right : null);

            if(tokenClass is IRightBracket)
            {
                NotImplementedMethod(tokenClass);
                return default;
            }

            if(tokenClass is ILeftBracket)
            {
                NotImplementedMethod(tokenClass);
                return default;
            }

            NotImplementedMethod(tokenClass);
            return default;
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

        internal int? GetBracketLevel()
        {
            if(!(TokenClass is IRightBracket rightParenthesis))
                return null;
            var leftParenthesis = Left?.TokenClass as ILeftBracket;
            return T(leftParenthesis?.Level ?? 0, rightParenthesis.Level).Max();
        }

        internal TContainer FlatFormat<TContainer, TValue>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = Token.Characters
                .FlatFormat(Left == null? null : Token.PrecededWith, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (IsSeparatorRequired? " " : "") + tokenString;

            var leftResult = Left == null
                ? new TContainer()
                : Left.FlatSubFormat<TContainer, TValue>(areEmptyLinesPossible);

            if(leftResult == null)
                return null;

            var rightResult = Right == null
                ? new TContainer()
                : Right.FlatSubFormat<TContainer, TValue>(areEmptyLinesPossible);

            if(rightResult == null)
                return null;

            return leftResult.Concat(tokenString, rightResult);
        }

        TContainer FlatSubFormat<TContainer, TValue>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => FlatFormat<TContainer, TValue>(areEmptyLinesPossible);

        public BinaryTree GetRightNeighbor(int current)
        {
            NotImplementedMethod(current);
            return default;
        }

        public bool HasAsParent(BinaryTree parent)
            => Parent
                .Chain(node => node.Depth >= parent.Depth? node.Parent : null)
                .Any(node => node == parent);
    }
}