using System.ComponentModel;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses.Whitespace;
using Reni.Validation;
using static Reni.Validation.IssueId;

namespace Reni.TokenClasses;

public sealed class BinaryTree : DumpableObject, ISyntax, ValueCache.IContainer, ITree<BinaryTree>
{
    internal interface IFormatter
    {
        void SetupPositions(IPositionTarget target);
        bool HasComplexDeclaration(BinaryTree target);
    }

    internal interface IPositionTarget;

    internal sealed class BracketNodes
    {
        internal BinaryTree Center;
        internal BinaryTree Left;
        internal BinaryTree Right;

        [DisableDump]
        internal Anchor ToAnchor => Anchor.Create(Left, Right);

        [DisableDump]
        internal BinaryTree[] Anchors => T(Left, Right);
    }

    static int NextObjectId;

    [EnableDump(Order = 1)]
    [EnableDumpExcept(null)]
    internal BinaryTree Left { get; }

    [EnableDump(Order = 3)]
    [EnableDumpExcept(null)]
    internal BinaryTree Right { get; }

    [DisableDump]
    internal readonly SourcePart Token;

    [DisableDump]
    internal readonly WhiteSpaceItem WhiteSpaces;

    [DisableDump]
    internal Syntax Syntax;

    [DisableDump]
    internal BinaryTree Parent;

    [PublicAPI]
    [EnableDump]
    [EnableDumpExcept(null)]
    internal IFormatter Formatter;

    [DisableDump]
    readonly ITokenClass InnerTokenClass;

    readonly FunctionCache<bool, string> FlatFormatCache;
    readonly FunctionCache<int, BinaryTree> ContainingTreeItemCache;

    [DisableDump]
    BinaryTree LeftNeighbor;

    [DisableDump]
    [UsedImplicitly]
    BinaryTree RightNeighbor;

    int Depth;

    [DisableDump]
    internal SourcePart FullToken => WhiteSpaces.SourcePart.Start.Span(Token.End);

    string InnerTokenClassPart => InnerTokenClass == TokenClass? "" : $"/{InnerTokenClass.Id}";

    [DisableDump]
    internal ITokenClass TokenClass => this.CachedValue(GetTokenClass);

    [DisableDump]
    internal SourcePart SourcePart
        => LeftMost
            .WhiteSpaces
            .SourcePart
            .Start
            .Span(RightMost.Token.End);

    [DisableDump]
    internal BinaryTree LeftMost => Left?.LeftMost ?? this;

    [DisableDump]
    internal BinaryTree RightMost => Right?.RightMost ?? this;

    [DisableDump]
    internal IEnumerable<Issue> AllIssues
        => T(Left?.AllIssues, Issues, Right?.AllIssues)
            .ConcatMany()
            .Where(node => node != null);

    [DisableDump]
    internal Issue[] Issues => this.CachedValue(GetIssue).NullableToArray().ToArray();

    [DisableDump]
    internal BracketNodes BracketKernel
    {
        get
        {
            if(TokenClass is not IIssueTokenClass errorToken)
                return TokenClass is IRightBracket
                    ? new BracketNodes { Left = Left, Center = Left.Right, Right = this }
                    : null;

            if(errorToken.IssueId == ExtraLeftBracket)
                return new() { Left = this, Center = Right, Right = RightMost };
            if(errorToken.IssueId == ExtraRightBracket)
                return new() { Left = Left.LeftMost, Center = Left, Right = this };
            if(errorToken.IssueId == MissingMatchingRightBracket)
                return new() { Left = Left, Center = Left.Right, Right = this };

            throw new InvalidEnumArgumentException($"Unexpected Bracket issue: {errorToken.IssueId}");
        }
    }

    [DisableDump]
    public BinaryTree[] ParserLevelGroup
        => this.CachedValue(() => GetParserLevelGroup()?.ToArray() ?? new BinaryTree[0]);

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
                return default;

            if(leftBracket != null)
            {
                if(Parent == null || Parent.IsBracketLevel)
                    return default;
                left.AssertIsNull();
                rightBracket.AssertIsNull();
                return ExtraLeftBracket;
            }

            rightBracket.AssertIsNotNull();
            var level = rightBracket.Level;

            right.AssertIsNull();

            var innerLeftBracket = left.InnerTokenClass as ILeftBracket;
            if(innerLeftBracket == null)
                return ExtraRightBracket;

            left.Left.AssertIsNull();

            return innerLeftBracket.Level > level? MissingMatchingRightBracket : default;
        }
    }

    bool IsBracketLevel => InnerTokenClass is IRightBracket;

    [DisableDump]
    internal SeparatorRequests SeparatorRequests
    {
        get
        {
            StopByObjectIds();
            var flat = SeparatorExtension.Get(LeftNeighbor?.InnerTokenClass, InnerTokenClass as ISeparatorClass);
            return new()
            {
                Head = SeparatorExtension.Get(LeftNeighbor?.InnerTokenClass, WhiteSpaces) //
                , Inner = true
                , Tail = SeparatorExtension.Get(WhiteSpaces, InnerTokenClass as ISeparatorClass) //
                , Flat = flat //
            };
        }
    }

    [DisableDumpExcept(true)]
    internal bool HasComplexDeclaration => Formatter?.HasComplexDeclaration(this) ?? false;

    BinaryTree
    (
        BinaryTree left
        , ITokenClass tokenClass
        , WhiteSpaceItem whiteSpaces
        , SourcePart token
        , BinaryTree right
    )
        : base(NextObjectId++)
    {
        Token = token;
        WhiteSpaces = whiteSpaces;
        Left = left;
        InnerTokenClass = tokenClass;
        Right = right;
        FlatFormatCache = new(GetFlatStringValue);
        ContainingTreeItemCache = new(position => GetContainingTreeItemForCache(Token.Source + position));

        SetLinks();
        StopByObjectIds();
        Issues.Any().ConditionalBreak(() => Issues.ToArray().LogDump());
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    SourcePart ISyntax.All => SourcePart;
    SourcePart ISyntax.Main => Token;
    int ITree<BinaryTree>.DirectChildCount => 2;

    BinaryTree ITree<BinaryTree>.GetDirectChild(int index)
        => index switch
        {
            0 => Left, 1 => Right, _ => null
        };

    int ITree<BinaryTree>.LeftDirectChildCount => 1;

    protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id}{InnerTokenClassPart})";


    Issue GetIssue()
    {
        if(TokenClass is not IIssueTokenClass errorToken)
            return null;

        if(errorToken.IssueId == ExtraLeftBracket)
            return errorToken.IssueId.GetIssue(Token, Right?.SourcePart ?? Token.End.Span(0));
        if(errorToken.IssueId == ExtraRightBracket)
            return errorToken.IssueId.GetIssue(Token, Left.SourcePart);
        if(errorToken.IssueId == MissingMatchingRightBracket)
            return errorToken.IssueId.GetIssue(Token, LeftMost.SourcePart);
        if(errorToken.IssueId == EOFInComment || errorToken.IssueId == EOLInString)
            return errorToken.IssueId.GetIssue(Token);

        throw new InvalidEnumArgumentException($"Unexpected issue: {errorToken.IssueId}");
    }

    ITokenClass GetTokenClass()
    {
        var issueId = BracketIssueId;
        return issueId == default? InnerTokenClass : IssueTokenClass.From[issueId];
    }

    BinaryTree GetContainingTreeItemForCache(SourcePosition position)
    {
        if(position < WhiteSpaces.SourcePart.Start)
            return Left?.ContainingTreeItemCache[position.Position];

        if(position < Token.End || (position == Token.End && TokenClass is EndOfText))
            return this;

        return Right?.ContainingTreeItemCache[position.Position];
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
            var elseItem = Parent is { TokenClass: ElseToken }? Parent : null;
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
        return new(left, tokenClass, new(token.GetPrefixSourcePart()), token.Characters, right);
    }

    internal BinaryTree ReCreate(BinaryTree[] left = null, BinaryTree[] right = null)
    {
        (left == null || left.Length <= 1).Assert();
        (right == null || right.Length <= 1).Assert();
        if((left == null || left[0] == Left) && (right == null || right[0] == Right))
            return this;

        return new(left == null? Left : left[0], TokenClass, WhiteSpaces, Token, right == null? Right : right[0]);
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
        var separatorRequests = SeparatorRequests;
        var tokenString = Left == null? "" : WhiteSpaces.FlatFormat(areEmptyLinesPossible, separatorRequests);

        if(tokenString == null)
            return null;

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

        var gapSeparatorRequests = GetGapSeparatorRequests();
        var gapString =
            Right == null
                ? ""
                : Right.LeftMost.WhiteSpaces.FlatFormat(areEmptyLinesPossible, gapSeparatorRequests);
        if(gapString == null)
            return null;

        return leftResult + tokenString + Token.Id + gapString + rightResult;
    }

    SeparatorRequests GetGapSeparatorRequests() => new()
    {
        Head = Token.Length > 0 //
        , Inner = true
        , Tail = Right != null && Right.LeftMost.Token.Length > 0
        , Flat = SeparatorExtension.Get(InnerTokenClass, Right?.LeftMost.InnerTokenClass) //
    };


    internal bool HasAsParent(BinaryTree parent)
        => Parent
            .Chain(node => node.Depth >= parent.Depth? node.Parent : null)
            .Any(node => node == parent);

    /// <summary>
    ///     Try to format target into one line.
    /// </summary>
    /// <param name="areEmptyLinesPossible"></param>
    /// <returns>The formatted line or null if target contains line breaks.</returns>
    internal string GetFlatString(bool areEmptyLinesPossible) => FlatFormatCache[areEmptyLinesPossible];

    /// <summary>
    ///     Get the line length of target when formatted as one line.
    /// </summary>
    /// <param name="areEmptyLinesPossible"></param>
    /// <returns>The line length calculated or null if target contains line breaks.</returns>
    internal int? GetFlatLength(bool areEmptyLinesPossible) => FlatFormatCache[areEmptyLinesPossible]?.Length;

    internal void SetSyntax(Syntax syntax)
    {
        if(Token.Source.Identifier == Compiler.PredefinedSource)
            return;
        (Syntax == null || Syntax == syntax).Assert(() => @$"
this: {Dump()}
Current: {Syntax.Dump()}
New: {syntax.Dump()}");
        Syntax = syntax;
    }

    internal BinaryTree GetContainingTreeItem(SourcePosition offset)
    {
        (Token.Source == offset.Source).Assert();
        return ContainingTreeItemCache[offset.Position];
    }

    internal BinaryTree GetCommonRoot(BinaryTree end)
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

    internal(BinaryTree token, WhiteSpaceItem item) GetContainingItem(SourcePosition offset)
    {
        var result = GetContainingTreeItem(offset);
        result.AssertIsNotNull();
        var item = result.WhiteSpaces.GetItem(offset);
        return (result, item);
    }
}