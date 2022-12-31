using System;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni;

sealed class ResultData
{
    internal Closures Closures;
    internal CodeBase Code;
    internal bool? IsHollow;
    internal Size Size;
    internal TypeBase Type;

    internal ResultData() { }

    internal ResultData
    (
        Category category,
        Func<TypeBase> getType,
        Func<CodeBase> getCode,
        Func<Closures> getClosures = null,
        Func<Size> getSize = null,
        Func<bool> getIsHollow = null,
        Root rootContext = null,
        Func<string> getObjectDump = null
    )
    {
        var isHollow = getIsHollow == null? null : new ValueCache<bool>(getIsHollow);
        var size = getSize == null? null : new ValueCache<Size>(getSize);
        var type = getType == null? null : new ValueCache<TypeBase>(getType);
        var code = getCode == null? null : new ValueCache<CodeBase>(getCode);
        var closures = getClosures == null? null : new ValueCache<Closures>(getClosures);

        if(category.HasType())
            Type = ObtainType(isHollow, size, type, code, rootContext, getObjectDump);

        if(category.HasCode())
            Code = ObtainCode(isHollow, size, type, code, getObjectDump);

        if(category.HasSize())
            Size = ObtainSize(isHollow, size, type, code, getObjectDump);

        if(category.HasClosures())
            Closures = ObtainClosures(isHollow, size, type, code, closures, getObjectDump);

        if(category.HasIsHollow())
            IsHollow = ObtainIsHollow(isHollow, size, type, code, getObjectDump);
    }

    static TypeBase ObtainType
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        Root rootContext,
        Func<string> getObjectDump
    )
    {
        if(getType != null)
            return getType.Value;
// ReSharper disable ExpressionIsAlwaysNull
        var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
        if(isHollow == true)
            return rootContext.VoidType;
        Tracer.AssertionFailed($"Type cannot be determined for {getObjectDump()}");
        return null;
    }

    static CodeBase ObtainCode
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        Func<string> getObjectDump
    )
    {
        if(getCode != null)
            return getCode.Value;
// ReSharper disable ExpressionIsAlwaysNull
        var isHollow = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
// ReSharper restore ExpressionIsAlwaysNull
        if(isHollow == true)
            return CodeBase.Void;
        Tracer.AssertionFailed($"Code cannot be determined for {getObjectDump()}");
        return null;
    }

    static Size ObtainSize
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        Func<string> getObjectDump
    )
    {
        var result = TryObtainSize(getIsHollow, getSize, getType, getCode);
        (result != null).Assert(() => $"Size cannot be determined for {getObjectDump()}");
        return result;
    }

    static Size TryObtainSize
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    )
    {
        if(getSize != null)
            return getSize.Value;
        if(getType != null)
            return getType.Value.Size;
        if(getCode != null)
            return getCode.Value.Size;
        if(getIsHollow != null && getIsHollow.Value)
            return Size.Zero;
        return null;
    }

    static bool ObtainIsHollow
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        Func<string> getObjectDump
    )
    {
        var result = TryObtainIsHollow(getIsHollow, getSize, getType, getCode);
        if(result != null)
            return result.Value;
        Tracer.AssertionFailed($"It cannot be obtained if it is hollow for {getObjectDump()}");
        return false;
    }

    static bool? TryObtainIsHollow
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode
    ) =>
        getIsHollow?.Value ?? getSize?.Value.IsZero ?? getType?.Value.IsHollow ?? getCode?.Value.IsEmpty;

    static Closures ObtainClosures
    (
        ValueCache<bool> getIsHollow,
        ValueCache<Size> getSize,
        ValueCache<TypeBase> getType,
        ValueCache<CodeBase> getCode,
        ValueCache<Closures> getArgs,
        Func<string> getObjectDump
    )
    {
        if(getArgs != null)
            return getArgs.Value;
        if(getCode != null)
            return getCode.Value.Closures;
        if(TryObtainIsHollow(getIsHollow, getSize, getType, getCode) == true)
            return Closures.Void();

        return null;
    }

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
        if(Equals(value, default))
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