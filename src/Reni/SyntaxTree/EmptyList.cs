using Reni.Basics;
using Reni.Context;
using Reni.Validation;

namespace Reni.SyntaxTree;

sealed class EmptyList : ValueSyntax.NoChildren
{
    readonly Issue? Issue;

    public EmptyList(Anchor anchor, Issue? issue = null)
        : base(anchor)
    {
        Issue = issue;
        anchor.AssertIsNotNull();
        StopByObjectIds();
        AssertValid();
    }

    protected override IEnumerable<Issue> GetIssues()
    {
        if(Issue != null)
            yield return Issue;
    }

    protected override string GetNodeDump() => "()";

    internal override Result GetResultForCache(ContextBase context, Category category)
        => context.RootContext.VoidType.GetResult(category);
}