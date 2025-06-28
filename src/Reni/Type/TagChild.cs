using Reni.Basics;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type;

abstract class TagChild<TParent> : Child<TParent>
    where TParent : TypeBase
{
    protected TagChild(TParent parent)
        : base(parent) { }

    [DisableDump]
    protected abstract string TagTitle { get; }

    [DisableDump]
    internal override string DumpPrintText => "(" + Parent.DumpPrintText + ")" + TagTitle;

    [DisableDump]
    internal sealed override bool IsHollow => Parent.IsHollow;

    [DisableDump]
    internal sealed override TypeBase TagTargetType => Parent.TagTargetType;

    protected sealed override Size GetSize() => Parent.Size;
    protected override string GetNodeDump() => Parent.NodeDump + "[" + TagTitle + "]";
    internal sealed override Result GetCleanup(Category category) => Parent.GetCleanup(category);
    internal sealed override Result GetCopier(Category category) => Parent.GetCopier(category);

    protected override Result ParentConversionResult(Category category)
        => GetMutation(Parent) & category;

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
    {
        get { yield return Feature.Extension.Conversion(ParentConversionResult); }
    }
}