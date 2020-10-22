using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class ErrorToken : DumpableObject, IToken
    {
        sealed class TokenClass : DumpableObject, ITokenClass
        {
            internal static readonly ITokenClass Instance = new TokenClass();
            public string Id => "<error>";
        }

        readonly Issue Issue;
        readonly SourcePosition Position;

        ErrorToken(Issue issue, SourcePosition position)
        {
            Issue = issue;
            Position = position;
        }

        SourcePart IToken.Characters => Position?.Span(0) ?? Issue.Position;
        bool? IToken.IsBracketAndLeftBracket => true;

        IEnumerable<IItem> IToken.PrecededWith => new IItem[0];

        internal static BinaryTree Create(Issue issue, BinaryTree target)
            => BinaryTree.Create(null, TokenClass.Instance, new ErrorToken(issue, target.Token.SourcePart().Start), null);
    }
}