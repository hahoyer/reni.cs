using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.Validation;

namespace Reni.Struct;

sealed class ForeignCodeFeature : DumpableObject, IImplementation, IMeta
{
    readonly Root Root;

    public ForeignCodeFeature(Root root) => Root = root;

    IFunction? IEvalImplementation.Function => null;
    public IValue? Value => null;

    Result IMeta.GetResult
    (
        Category category
        , ResultCache left
        , SourcePart token
        , ContextBase context
        , ValueSyntax? right
    )
    {
        if(right == null)
            return
                new(category,
                    IssueId
                        .MissingForeignFunctionSpecification
                        .GetIssue(Root, token, left.Type));
        var args = right.AssertNotNull();
        var module = left.Get(Category.All).GetValueAsText(Root.ExecutionContext).ToString(Size.TextItemSize);
        var entry = args.GetResultForAll(context).GetValueAsText(Root.ExecutionContext).ToString(Size.TextItemSize);

        return Root.GetForeignCodeType(module, entry).GetResult(category);
    }

    IMeta IMetaImplementation.Function => this;
}