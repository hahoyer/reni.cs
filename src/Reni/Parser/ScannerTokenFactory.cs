using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser;

sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<BinaryTree>, Compiler<BinaryTree>.IComponent
{
    bool IsSubParser { get; }

    [EnableDump]
    Compiler<BinaryTree>.Component Current;

    LexerItem[] Classes =>
    [
        Lexer.Instance.WhiteSpacesItem
        , Lexer.Instance.InlineCommentItem
        , new(new Number(), Lexer.Instance.MatchNumber)
        , new(Current.Get<ScannerTokenType<BinaryTree>>(), Lexer.Instance.MatchAny)
        , new(new Text(), Lexer.Instance.MatchText)
    ];

    public ScannerTokenFactory(bool isSubParser = false) => IsSubParser = isSubParser;

    Compiler<BinaryTree>.Component Compiler<BinaryTree>.IComponent.Current
    {
        set => Current = value;
    }

    LexerItem[] ITokenFactory.Classes => Classes;
    IScannerTokenType ITokenFactory.EndOfText => IsSubParser? EndOfSubText.Instance : EndOfText.Instance;

    IScannerTokenType ITokenFactory.InvalidCharacterError
        => Lexer.ConvertError(IssueId.InvalidCharacter);

    IParserTokenType<BinaryTree> ITokenFactory<BinaryTree>.BeginOfText => BeginOfText.Instance;
}