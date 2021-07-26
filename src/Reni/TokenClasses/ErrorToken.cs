using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class ErrorToken : DumpableObject, IToken
    {
        sealed class TokenClass : DumpableObject, ITokenClass, IErrorToken
        {
            readonly IssueId IssueId;
            public TokenClass(IssueId issueId) => IssueId = issueId;

            IssueId IErrorToken.IssueId => IssueId;
            public string Id => IssueId.Tag;
        }

        readonly SourcePosition Position;

        ErrorToken(SourcePosition position) => Position = position;

        SourcePart IToken.Characters => Position.Span(0);
        bool? IToken.IsBracketAndLeftBracket => true;

        IEnumerable<hw.Scanner.IItem> IToken.PrecededWith => new hw.Scanner.IItem[0];

        internal static BinaryTree CreateTreeItem(IssueId issueId, SourcePosition target)
            => BinaryTree.Create(null, new TokenClass(issueId), new ErrorToken(target), null);

        internal static BinaryTree CreateTreeItem(IssueId issueId, BinaryTree target)
            => CreateTreeItem(issueId, target.Token.SourcePart().Start);

        internal static EmptyList CreateSyntax(IssueId issueId, SourcePosition position)
            => new(Anchor.Create(CreateTreeItem(issueId, position)));

        internal static ValueSyntax CreateSyntax(IssueId issueId, ValueSyntax target)
        {
            NotImplementedFunction(issueId, target);
            return target;
        }
    }
}