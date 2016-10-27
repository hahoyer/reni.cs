using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory, CompilerBase.IComponent
    {
        CompilerBase.Component Current;

        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        LexerItem[] Classes => new[]
        {
            Lexer.Instance.WhiteSpaceItem,
            Lexer.Instance.LineEndItem,
            Lexer.Instance.CommentItem,
            Lexer.Instance.LineCommentItem,
            new LexerItem(new Number(), Lexer.Instance.Number),
            new LexerItem(Current.Get<GenericTokenFactory<Syntax>>(), Lexer.Instance.Any),
            new LexerItem(new Text(), Lexer.Instance.Text)
        };

        CompilerBase.Component CompilerBase.IComponent.Current { set { Current = value; } }
    }
}