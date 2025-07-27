using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class IssueType : TypeBase, IContextReference
{
    readonly Issue Issue;
    internal override Issue[] Issues => [Issue];

    [DisableDump]
    internal override Root Root { get; }

    public IssueType(Root root, Issue issue)
    {
        Issue = issue;
        Root = root;
        StopByObjectIds();
    }

    int IContextReference.Order => default;

    protected override TypeBase GetTypeType() => this;

    [DisableDump]
    internal override bool IsHollow => true;

    protected override Size GetSize() => Size.Zero;

    internal override Result GetInstanceResult(Category category, Func<Category, Result> getRightResult)
        => GetPair(Root.GetIssueType(getRightResult(category).Issues)).GetResult(category);

    protected override CodeBase DumpPrintCode => new DumpPrintText(GetType().PrettyName());

    [DisableDump]
    protected override IssueId MissingDeclarationIssueId => IssueId.ConsequenceError;
}
