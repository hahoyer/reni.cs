using Reni.Basics;
using Reni.Feature;

namespace Reni.Type;

abstract class TagChild<TParent> : Child<TParent>
    where TParent : TypeBase
{
    protected TagChild(TParent parent)
        : base(parent) { }

    [DisableDump]
    protected abstract string TagTitle { get; }

    protected override string GetDumpPrintText() => "(" + Parent.OverView.DumpPrintText + ")" + TagTitle;

    protected sealed override bool GetIsHollow() => Parent.OverView.IsHollow;

    protected sealed override TypeBase GetTagTargetType() => Parent.Make.TagTargetType;

    protected sealed override Size GetSize() => Parent.OverView.Size;
    protected override string GetNodeDump() => Parent.NodeDump + "[" + TagTitle + "]";
    internal sealed override Result GetCleanup(Category category) => Parent.GetCleanup(category);
    internal sealed override Result GetCopier(Category category) => Parent.GetCopier(category);

    protected override Result ParentConversionResult(Category category)
        => GetMutation(Parent) & category;

    protected override IEnumerable<IConversion> GetStripConversions()
    {
        yield return Feature.Extension.Conversion(ParentConversionResult);
    }
}
