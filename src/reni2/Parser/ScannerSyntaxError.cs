using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerSyntaxError : ParserTokenType<BinaryTree>, ITokenClass, IValueProvider
    {
        readonly IssueId IssueId;

        public ScannerSyntaxError(IssueId issueId)
        {
            IssueId = issueId;
            StopByObjectIds(81);
        }

        string ITokenClass.Id => Id;

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            if(binaryTree.Right == null)
            {
                var issues = IssueId.Issue(binaryTree.Token.Characters);
                if(binaryTree.Left == null)
                    return new Result<ValueSyntax>(new EmptyList(binaryTree), issues);
                else
                return binaryTree.Left.GetValue(scope).With(issues);
            }

            NotImplementedMethod(binaryTree);
            return null;
        }

        public override string Id => "<error>";

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);
    }
}