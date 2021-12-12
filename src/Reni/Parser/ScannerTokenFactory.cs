using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<BinaryTree>, Compiler<BinaryTree>.IComponent
    {
        bool IsSubParser { get; }

        public ScannerTokenFactory(bool isSubParser = false) => IsSubParser = isSubParser;

        [EnableDump]
        Compiler<BinaryTree>.Component Current;

        Compiler<BinaryTree>.Component Compiler<BinaryTree>.IComponent.Current
        {
            set => Current = value;
        }

        IParserTokenType<BinaryTree> ITokenFactory<BinaryTree>.BeginOfText => BeginOfText.Instance;
        IScannerTokenType ITokenFactory.EndOfText => IsSubParser? EndOfSubText.Instance: EndOfText.Instance;

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        LexerItem[] Classes => new[]
        {
            Lexer.Instance.WhiteSpacesItem,
            Lexer.Instance.InlineCommentItem,
            new LexerItem(new Number(), Lexer.Instance.MatchNumber),
            new LexerItem(Current.Get<ScannerTokenType<BinaryTree>>(), Lexer.Instance.MatchAny),
            new LexerItem(new Text(), Lexer.Instance.MatchText)
        };
    }
}