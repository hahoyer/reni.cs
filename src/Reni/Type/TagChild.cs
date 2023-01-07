using hw.DebugFormatter;
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

    [DisableDump]
    internal override string DumpPrintText => "(" + Parent.DumpPrintText + ")" + TagTitle;

    [DisableDump]
    internal sealed override bool IsHollow => Parent.IsHollow;

    [DisableDump]
    internal sealed override TypeBase CoreType => Parent.CoreType;

    protected sealed override Size GetSize() => Parent.Size;
    protected override string GetNodeDump() => Parent.NodeDump + "[" + TagTitle + "]";
    internal sealed override Result Cleanup(Category category) => Parent.Cleanup(category);
    internal sealed override Result Copier(Category category) => Parent.Copier(category);

    protected override Result ParentConversionResult(Category category)
        => Mutation(Parent) & category;

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
    {
        get { yield return Feature.Extension.Conversion(ParentConversionResult); }
    }
}