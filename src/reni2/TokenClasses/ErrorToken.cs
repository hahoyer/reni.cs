using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class ErrorToken : DumpableObject, IToken
    {
        sealed class TokenClass : DumpableObject, ITokenClass
        {
            internal static readonly ITokenClass Instance = new TokenClass();
            public string Id => "<error>";
        }

        readonly SourcePosition Position;

        ErrorToken(SourcePosition position) => Position = position;
        SourcePart IToken.Characters => Position.Span(0);
        bool? IToken.IsBracketAndLeftBracket => true;

        IEnumerable<IItem> IToken.PrecededWith => new IItem[0];

        public static BinaryTree Create(BinaryTree target)
            => BinaryTree.Create(null, TokenClass.Instance, new ErrorToken(target.Token.SourcePart().Start), null);
    }
}