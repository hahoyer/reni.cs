using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.Validation;

namespace Reni.TokenClasses;

sealed class EndOfText
    : TokenClass
        , IBracketMatch<BinaryTree>
        , ISyntaxScope
        , IBelongingsMatcher
        , IRightBracket
        , IValueToken
        , IIssueTokenClass

{
    sealed class Matched : DumpableObject, IParserTokenType<BinaryTree>
    {
        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree? left, IToken token, BinaryTree? right)
        {
            right.AssertIsNull();
            left.AssertIsNotNull();
            left!.Right.AssertIsNull();
            left.Left.AssertIsNotNull();
            left.Left!.Left.AssertIsNull();
            return left;
        }

        string IParserTokenType<BinaryTree>.PrioTableId => "()";
    }

    const string TokenId = PrioTable.EndOfText;
    public static readonly EndOfText Instance = new(IssueId.None);
    public static readonly EndOfText ErrorInstance = new(IssueId.EOFInComment);
    readonly IssueId IssueId;

    EndOfText(IssueId issueId) => IssueId = issueId;

    bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
        => otherMatcher is BeginOfText;

    int IBracket.Level => 0;

    IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

    IssueId IIssueTokenClass.IssueId => IssueId;

    [DisableDump]
    public override string Id => TokenId;

    IValueProvider IValueToken.Provider => Factory.Bracket;
}

sealed class EndOfSubText
    : TokenClass
        , ISyntaxScope

{
    const string TokenId = PrioTable.EndOfText;
    public static readonly EndOfSubText Instance = new();

    EndOfSubText() { }

    [DisableDump]
    public override string Id => TokenId;
}

sealed class BeginOfText : TokenClass, IBelongingsMatcher, ILeftBracket
{
    const string TokenId = PrioTable.BeginOfText;
    public static readonly BeginOfText Instance = new();

    bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
        => otherMatcher is EndOfText;

    int IBracket.Level => 0;

    [DisableDump]
    public override string Id => TokenId;
}