using hw.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerSyntaxError : ParserTokenType<BinaryTree>, IIssueTokenClass, IValueToken
    {
        internal readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId) => IssueId = issueId;

        public override string Id => "<error>";

        string ITokenClass.Id => $"<error:{IssueId}>";

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);

        IssueId IIssueTokenClass.IssueId => IssueId;

        IValueProvider IValueToken.Provider => Factory.ScannerError;
    }
}