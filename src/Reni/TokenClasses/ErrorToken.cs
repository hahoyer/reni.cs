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
        sealed class TokenClass : DumpableObject, ITokenClass, IErrorToken
        {
            public string Id => IssueId.Tag;
            readonly IssueId IssueId;
            public TokenClass(IssueId issueId) => IssueId = issueId;

            IssueId IErrorToken.IssueId => IssueId;
        }

        readonly SourcePosition Position;

        ErrorToken(SourcePosition position) => Position = position;

        SourcePart IToken.Characters => Position.Span(0);
        bool? IToken.IsBracketAndLeftBracket => true;

        IEnumerable<IItem> IToken.PrecededWith => new IItem[0];

        internal static BinaryTree Create(IssueId issueId, BinaryTree target)
            => BinaryTree.Create(null, new TokenClass(issueId), new ErrorToken(target.Token.SourcePart().Start), null);

    }
}