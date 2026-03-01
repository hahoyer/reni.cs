using Reni.Basics;
using Reni.Feature;
using Reni.Helper;

namespace Reni.Type;

sealed class TextType : Child<TypeBase>
{
    public TextType(TypeBase parent)
        : base(parent)
    {
    }

    protected override Result ParentConversionResult(Category category)
    {
        NotImplementedMethod(category);
        return default!;
    }

    internal override object GetDataValue(BitsConst data)
        => data.ToString(Parent.ElementType.OverView.Size);

    protected override IEnumerable<IConversion> GetStripConversions()
    {
        yield return Feature.Extension.Conversion(NoTextItemResult);
    }

    Result NoTextItemResult(Category category)
        => GetResultFromPointer(category, Parent);
}
