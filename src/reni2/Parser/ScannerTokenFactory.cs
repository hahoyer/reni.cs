using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<BinaryTree>, Compiler<BinaryTree>.IComponent
    {
        [EnableDump]
        Compiler<BinaryTree>.Component Current;

        Compiler<BinaryTree>.Component Compiler<BinaryTree>.IComponent.Current
        {
            set => Current = value;
        }

        IParserTokenType<BinaryTree> ITokenFactory<BinaryTree>.BeginOfText => new BeginOfText();
        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        LexerItem[] Classes => new[]
        {
            Lexer.Instance.SpaceItem,
            Lexer.Instance.LineEndItem,
            Lexer.Instance.MultiLineCommentItem,
            Lexer.Instance.LineCommentItem,
            new LexerItem(new Number(), Lexer.Instance.MatchNumber),
            new LexerItem(Current.Get<ScannerTokenType<BinaryTree>>(), Lexer.Instance.MatchAny),
            new LexerItem(new Text(), Lexer.Instance.MatchText)
        };
    }
}