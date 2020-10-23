using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerSyntaxError : ParserTokenType<BinaryTree>, ITokenClass, IErrorToken
    {
        internal readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId) => IssueId = issueId;

        public override string Id => "<error>";

        string ITokenClass.Id => Id;

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);

        IssueId IErrorToken.IssueId => IssueId;
    }

    interface IErrorToken
    {
        IssueId IssueId { get; }
    }
}