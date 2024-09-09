using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser;

sealed class InvalidCharacterSymbol : Definable
{
    readonly IssueId IssueId;
    public InvalidCharacterSymbol(IssueId issueId) => IssueId = issueId;

    public override string Id => "<InvalidCharacter>";
}