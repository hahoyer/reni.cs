using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni;

sealed class ResultData
{
    internal Closures Closures;
    internal CodeBase Code;
    internal bool? IsHollow;
    internal Size Size;
    internal TypeBase Type;

    public void Reset(Category category)
    {
        if(category.HasIsHollow())
            IsHollow = null;
        if(category.HasSize())
            Size = null;
        if(category.HasType())
            Type = null;
        if(category.HasCode())
            Code = null;
        if(category.HasClosures())
            Closures = null;
    }

    public void Set(Category category, object value)
    {
        if(Equals(value, null))
            Reset(category);
        else
        {
            if(category.HasIsHollow())
                IsHollow = (bool?)value;
            if(category.HasSize())
                Size = (Size)value;
            if(category.HasType())
                Type = (TypeBase)value;
            if(category.HasCode())
                Code = (CodeBase)value;
            if(category.HasClosures())
                Closures = (Closures)value;
        }
    }
}