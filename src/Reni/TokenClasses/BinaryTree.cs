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

        [DisableDump]
        internal ITokenClass TokenClass { get; }

        [DisableDump]
        internal BinaryTree LeftNeighbor;

        [DisableDump]
        internal BinaryTree Parent;

        [DisableDump]
        internal BinaryTree RightNeighbor;

        int Depth;

        readonly FunctionCache<bool, string> FlatFormatCache;

        BinaryTree
        (
            BinaryTree left
            , ITokenClass tokenClass
            , IToken token
            , BinaryTree right
        )
            : base(NextObjectId++)
        {
            Token = token;
            Left = left;
            TokenClass = tokenClass;
            Right = right;
            FlatFormatCache = new(GetFlatStringValue);

            SetLinks();
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        SourcePart ISyntax.All => SourcePart;
        SourcePart ISyntax.Main => Token.Characters;
        int ITree<BinaryTree>.DirectChildCount => 2;

        BinaryTree ITree<BinaryTree>.GetDirectChild(int index)
            => index switch
            {
                0 => Left, 1 => Right, _ => null
            };

        int ITree<BinaryTree>.LeftDirectChildCount => 1;

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id})";


        [DisableDump]
        internal SourcePart SourcePart => LeftMost.Token.SourcePart().Start.Span(RightMost.Token.Characters.End);

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
                if (!(TokenClass is IRightBracket rightParenthesis))
                {
                    if (TokenClass is ILeftBracket leftBracket)
                    {
                        Left.AssertIsNull();
                        return new()
                        {
                            Left = this, Center = Right
                            , Right = ErrorToken.Create(IssueId.MissingRightBracket, RightMost)
                        };
                    }

                    return null;
                }

                var level = rightParenthesis.Level;

                (Right == null).Assert();

                var result = new BracketNodes {Left = Left, Center = Left.Right, Right = this};

                if (!(Left.TokenClass is ILeftBracket leftParenthesis))
                {
                    result.Center = Left;
                    result.Left = ErrorToken.Create(IssueId.MissingLeftBracket, Left.LeftMost);
                    return result;
                }

                (Left.Left == null).Assert();

                var levelDelta = leftParenthesis.Level - level;

                if (levelDelta == 0)
                    return result;

                if (levelDelta > 0)
                {
                    result.Right = ErrorToken.Create(IssueId.MissingRightBracket, RightMost);
                    return result;
                }

                Left.NotImplementedMethod(level, this);
                return null;
            }
        }

        [DisableDump]
        public BinaryTree[] ParserLevelGroup => this.CachedValue(() => GetParserLevelGroup(TokenClass).ToArray());

        [DisableDump]
        public bool IsSeparatorRequired
            => !Token.PrecededWith.HasComment() && SeparatorExtension.Get(LeftNeighbor?.TokenClass, TokenClass);

        /// <summary>
        ///     Get the line length of target when formatted as one line.
        /// </summary>
        /// <value>The line length calculated or null if target contains line breaks.</value>
        internal int FlatLengthOfToken => (IsSeparatorRequired ? 1 : 0) + Token.Characters.Length;

        internal string FlatStringWithLines => FlatFormatCache[false];
        internal string FlatStringWithoutLines => FlatFormatCache[true];

        void SetLinks()
        {
            if (Left != null)
            {
                Left.Parent = this;
                Left.Depth = Depth + 1;
                var binaryTree = Left.Chain(node => node.Right).Last();
                binaryTree.RightNeighbor = this;
                LeftNeighbor = binaryTree;
            }

            if (Right != null)
            {
                Right.Parent = this;
                Right.Depth = Depth + 1;
                var binaryTree = Right.Chain(node => node.Left).Last();
                binaryTree.LeftNeighbor = this;
                RightNeighbor = binaryTree;
            }
        }

        IEnumerable<BinaryTree> GetParserLevelGroup(ITokenClass tokenClass)
        {
            if (tokenClass is not IBelongingsMatcher)
                return new BinaryTree[0];

            if (tokenClass is List)
                return this
                    .Chain(node => tokenClass.IsBelongingTo(node.Right?.TokenClass) ? node.Right : null);

            if (tokenClass is IRightBracket)
            {
                NotImplementedMethod(tokenClass);
                return default;
            }

            if (tokenClass is ILeftBracket)
            {
                NotImplementedMethod(tokenClass);
                return default;
            }

            NotImplementedMethod(tokenClass);
            return default;
        }

        internal static BinaryTree Create
        (
            BinaryTree left
            , ITokenClass tokenClass
            , IToken token
            , BinaryTree right
        )
            => new(left, tokenClass, token, right);

        internal int? GetBracketLevel()
        {
            if (!(TokenClass is IRightBracket rightParenthesis))
                return null;
            var leftParenthesis = Left?.TokenClass as ILeftBracket;
            return T(leftParenthesis?.Level ?? 0, rightParenthesis.Level).Max();
        }

        string GetFlatStringValue(bool areEmptyLinesPossible)
        {
            var tokenString = Token.Characters
                .FlatFormat(LeftNeighbor == null ? null : Token.PrecededWith, areEmptyLinesPossible);

            if (tokenString == null)
                return null;

            tokenString = (IsSeparatorRequired ? " " : "") + tokenString;

            var leftResult = Left == null
                ? ""
                : Left.FlatFormatCache[areEmptyLinesPossible];

            if (leftResult == null)
                return null;

            var rightResult = Right == null
                ? ""
                : Right.FlatFormatCache[areEmptyLinesPossible];

            if (rightResult == null)
                return null;

            return leftResult + tokenString + rightResult;
        }


        public bool HasAsParent(BinaryTree parent)
            => Parent
                .Chain(node => node.Depth >= parent.Depth ? node.Parent : null)
                .Any(node => node == parent);

        /// <summary>
        ///     Try to format target into one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The formatted line or null if target contains line breaks.</returns>
        public string GetFlatString(bool areEmptyLinesPossible) => FlatFormatCache[areEmptyLinesPossible];

        /// <summary>
        ///     Get the line length of target when formatted as one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The line length calculated or null if target contains line breaks.</returns>
        internal int? GetFlatLength(bool areEmptyLinesPossible) => FlatFormatCache[areEmptyLinesPossible]?.Length;
    }
}