using hw.Parser;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser;

sealed class ScannerSyntaxError : ParserTokenType<BinaryTree>, IIssueTokenClass, IValueToken
{
    internal readonly IssueId IssueId;
    
    internal ScannerSyntaxError(IssueId issueId) => IssueId = issueId;
    
    IssueId IIssueTokenClass.IssueId => IssueId;
    string ITokenClass.Id => $"<error:{IssueId}>";
    IValueProvider IValueToken.Provider => Factory.ScannerError;
    
    public override string Id => "<error>";

    protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
        => BinaryTree.Create(left, this, token, right);
}