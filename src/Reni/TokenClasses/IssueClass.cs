using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses;

sealed class IssueTokenClass : DumpableObject, IIssueTokenClass
{
    internal static readonly FunctionCache<IssueId, IssueTokenClass> From = new(issueId => new(issueId));

    readonly IssueId IssueId;
    IssueTokenClass(IssueId issueId) => IssueId = issueId;

    IssueId IIssueTokenClass.IssueId => IssueId;
    string ITokenClass.Id => $"<issue:{IssueId}>";
}