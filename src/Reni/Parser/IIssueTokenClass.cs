using Reni.Validation;

namespace Reni.Parser
{
    interface IIssueTokenClass: ITokenClass
    {
        IssueId IssueId { get; }
    }
}