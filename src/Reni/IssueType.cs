using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class IssueType(Root root, Issue[] issues) : TypeBase, IContextReference
{
    internal override Issue[] Issues { get; } = issues;

    [DisableDump]
    internal override Root Root { get; } = root;

    int IContextReference.Order => default;

    internal override TypeBase TypeType => this;

    [DisableDump]
    internal override bool IsHollow => true;

    protected override Size GetSize() => Size.Zero;

    internal override Result GetInstanceResult(Category category, Func<Category, Result> getRightResult)
        => new(category, Issues.Concat(getRightResult(category).Issues).ToArray());

    protected override CodeBase DumpPrintCode => new DumpPrintText(GetType().PrettyName());

    protected override Issue GetMissingDeclarationIssue(SourcePart position)
        => IssueId.ConsequenceError.GetIssue(Root, position);
}