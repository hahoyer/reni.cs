using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.Validation;
using static Reni.Validation.IssueId;

namespace Reni.TokenClasses
{
    public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, ITree<BinaryTree>
    {
        internal sealed class BracketNodes
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
        internal Syntax Syntax;

        [DisableDump]
        readonly ITokenClass InnerTokenClass;

        readonly FunctionCache<bool, string> FlatFormatCache;
        readonly FunctionCache<int, BinaryTree> LocationCache;

        readonly ValueCache<ITokenClass> TokenClassCache;

        [DisableDump]
        BinaryTree LeftNeighbor;

        [DisableDump]
        BinaryTree Parent;

        [DisableDump]
        BinaryTree RightNeighbor;

        int Depth;

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
            InnerTokenClass = tokenClass;
            Right = right;
            FlatFormatCache = new(GetFlatStringValue);
            LocationCache = new(GetItemByOffset);
            TokenClassCache = new(GetTokenClass);

            SetLinks();
            StopByObjectIds();
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

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id}{InnerTokenClassPart})";

        string InnerTokenClassPart => InnerTokenClass == TokenClass? "" : $"/{InnerTokenClass.Id}";

        [DisableDump]
        internal ITokenClass TokenClass => this.CachedValue(GetTokenClass);

        [DisableDump]
        internal SourcePart SourcePart => LeftMost.Token.SourcePart().Start.Span(RightMost.Token.Characters.End);

        [DisableDump]
        internal BinaryTree LeftMost => Left?.LeftMost ?? this;

        [DisableDump]
        BinaryTree RightMost => Right?.RightMost ?? this;

        [DisableDump]
        internal IEnumerable<Issue> AllIssues
            => T(Left?.AllIssues, T(Issue), Right?.AllIssues)
                .ConcatMany()
                .Where(node => node != null);

        [DisableDump]
        internal Issue Issue => this.CachedValue(GetIssue);

        [DisableDump]
        internal BracketNodes BracketKernel
        {
            get
            {
                if(TokenClass is not IIssueTokenClass errorToken)
                    return TokenClass is IRightBracket
                        ? new BracketNodes { Left = Left, Center = Left.Right, Right = this }
                        : null;

                if(errorToken.IssueId == MissingRightBracket)
                    return new() { Left = this, Center = Right, Right = RightMost };
                if(errorToken.IssueId == MissingLeftBracket)
                    return new() { Left = Left.LeftMost, Center = Left, Right = this };
                if(errorToken.IssueId == MissingMatchingRightBracket)
                    return new() { Left = Left, Center = Left.Right, Right = this };

                throw new InvalidEnumArgumentException($"Unexpected Bracket issue: {errorToken.IssueId}");
            }
        }

        [DisableDump]
        public BinaryTree[] ParserLevelGroup
            => this.CachedValue(() => GetParserLevelGroup().ToArray());

        [DisableDump]
        public bool IsSeparatorRequired
            => !Token.PrecededWith.HasComment() &&
                SeparatorExtension.Get(LeftNeighbor?.InnerTokenClass, InnerTokenClass);

        [DisableDump]
        IssueId BracketIssueId
        {
            get
            {
                var left = Left;
                var right = Right;
                var tokenClass = InnerTokenClass;
                var leftBracket = tokenClass as ILeftBracket;
                var rightBracket = tokenClass as IRightBracket;

                if(rightBracket == null && leftBracket == null)
                    return null;

                if(leftBracket != null)
                {
                    if(Parent is { IsBracketLevel: true })
                        return null;
                    left.AssertIsNull();
                    rightBracket.AssertIsNull();
                    return MissingRightBracket;
                }

                rightBracket.AssertIsNotNull();
                var level = rightBracket.Level;

                right.AssertIsNull();

                var innerLeftBracket = left.InnerTokenClass as ILeftBracket;
                if(innerLeftBracket == null)
                    return MissingLeftBracket;

                left.Left.AssertIsNull();

                return innerLeftBracket.Level > level? MissingMatchingRightBracket : null;
            }
        }

        bool IsBracketLevel => InnerTokenClass is IRightBracket;

        Issue GetIssue()
        {
            if(TokenClass is not IIssueTokenClass errorToken)
                return null;

            if(errorToken.IssueId == MissingRightBracket)
                return errorToken.IssueId.Issue(Right?.SourcePart ?? Token.Characters.End.Span(0));
            if(errorToken.IssueId == MissingLeftBracket)
                return errorToken.IssueId.Issue(Left.SourcePart);
            if(errorToken.IssueId == MissingMatchingRightBracket)
                return errorToken.IssueId.Issue(Left.Right.SourcePart);
            if(errorToken.IssueId == EOFInComment || errorToken.IssueId == EOLInString)
                return errorToken.IssueId.Issue(Token.Characters);

            throw new InvalidEnumArgumentException($"Unexpected issue: {errorToken.IssueId}");
        }

        ITokenClass GetTokenClass()
        {
            var issueId = BracketIssueId;
            return issueId == null? InnerTokenClass : IssueTokenClass.From[issueId];
        }

        BinaryTree GetItemByOffset(int position)
        {
            if(TokenClass is not EndOfText && Token.Characters.EndPosition <= position)
                return Right?.LocationCache[position];

            if(Token.Characters.Position <= position)
                return this;

            var whiteSpaceStart = Token.PrecededWith.FirstOrDefault();
            if(whiteSpaceStart != null && whiteSpaceStart.SourcePart.Position <= position)
                return this;

            return Left?.LocationCache[position];
        }

        void SetLinks()
        {
            if(Left != null)
            {
                Left.Parent = this;
                Left.Depth = Depth + 1;
                var binaryTree = Left.Chain(node => node.Right).Last();
                binaryTree.RightNeighbor = this;
                LeftNeighbor = binaryTree;
            }

            if(Right != null)
            {
                Right.Parent = this;
                Right.Depth = Depth + 1;
                var binaryTree = Right.Chain(node => node.Left).Last();
                binaryTree.LeftNeighbor = this;
                RightNeighbor = binaryTree;
            }
        }

        IEnumerable<BinaryTree> GetParserLevelGroup()
        {
            var tokenClass = InnerTokenClass;

            if(tokenClass is not IBelongingsMatcher)
                return new BinaryTree[0];

            if(tokenClass is List)
                return this
                    .Chain(node => tokenClass.IsBelongingTo(node.Right?.InnerTokenClass)? node.Right : null);

            if(tokenClass is IRightBracket)
                return BracketKernel == null? null : T(BracketKernel.Left, BracketKernel.Right);

            if(tokenClass is ILeftBracket)
                return Parent?.BracketKernel == null? null : T(Parent.BracketKernel.Left, Parent.BracketKernel.Right);

            if(tokenClass is ThenToken)
            {
                var elseItem = Parent is { TokenClass: ElseToken }? Parent: null;
                return elseItem == null? default : T(this, elseItem);
            }

            if(tokenClass is ElseToken)
            {
                var thenItem = Right.TokenClass is ThenToken? Right : null;
                return thenItem == null? default : T(thenItem, this);
            }

            NotImplementedMethod();
            return default;
        }

        internal static BinaryTree Create
        (
            BinaryTree left
            , ITokenClass tokenClass
            , IToken token
            , BinaryTree right
        )
        {
            var linked = token as ILinked<BinaryTree>;
            linked.AssertIsNotNull();
            linked.Container.AssertIsNull();
            return new(left, tokenClass, token, right);
        }

        internal int? GetBracketLevel()
        {
            if(InnerTokenClass is not IRightBracket rightParenthesis)
                return null;
            var leftParenthesis = Left?.InnerTokenClass as ILeftBracket;
            return T(leftParenthesis?.Level ?? 0, rightParenthesis.Level).Max();
        }

        string GetFlatStringValue(bool areEmptyLinesPossible)
        {
            var tokenString = Token.Characters.Id
                .FlatFormat(Left == null? null : Token.PrecededWith, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (IsSeparatorRequired? " " : "") + tokenString;

            var leftResult = Left == null
                ? ""
                : Left.FlatFormatCache[areEmptyLinesPossible];
            if(leftResult == null)
                return null;

            var rightResult = Right == null
                ? ""
                : Right.FlatFormatCache[areEmptyLinesPossible];
            if(rightResult == null)
                return null;

            var gapString =
                Right == null? "" : "".FlatFormat(Right.LeftMost.Token.PrecededWith, areEmptyLinesPossible);
            if(gapString == null)
                return null;

            return leftResult + tokenString + gapString + rightResult;
        }


        public bool HasAsParent(BinaryTree parent)
            => Parent
                .Chain(node => node.Depth >= parent.Depth? node.Parent : null)
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

        internal void SetSyntax(Syntax syntax)
        {
            if(Token.Characters.Source.Identifier == Compiler.PredefinedSource)
                return;
            (Syntax == null || Syntax == syntax).Assert(() => @$"
this: {Dump()}
Current: {Syntax.Dump()}
New: {syntax.Dump()}");
            Syntax = syntax;
        }

        internal BinaryTree LocateByPosition(SourcePosition offset)
        {
            (Token.Characters.Source == offset.Source).Assert();
            return LocationCache[offset.Position];
        }

        internal BinaryTree CommonRoot(BinaryTree end)
        {
            var startParents = this.Chain(node => node.Parent).Reverse().ToArray();
            var endParents = end.Chain(node => node.Parent).Reverse().ToArray();

            var result = startParents[0];
            for(var index = 1; index < startParents.Length && index < endParents.Length; index++)
            {
                var parent = startParents[index];
                if(parent != endParents[index])
                    return result;
                result = parent;
            }

            return result;
        }
    }
}