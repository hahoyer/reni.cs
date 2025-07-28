using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni;

static class ResultExtension
{
    static TypeBase? GetType
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode,
        Func<string>? getObjectDump
    )
    {
        if(getType != null)
            return getType.Value;
        Tracer.AssertionFailed($"Type cannot be determined for {getObjectDump!()}");
        return null;
    }

    static CodeBase? GetCode
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode,
        Func<string>? getObjectDump
    )
    {
        if(getCode != null)
            return getCode.Value;
        var isHollow = TryGetIsHollow(getIsHollow, getSize, getType, getCode);
        if(isHollow == true)
            return CodeBase.Void;
        Tracer.AssertionFailed($"Code cannot be determined for {getObjectDump!()}");
        return null;
    }

    static Size? GetSize
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode,
        Func<string>? getObjectDump
    )
    {
        var result = TryGetSize(getIsHollow, getSize, getType, getCode);
        (result != null).Assert(() => $"Size cannot be determined for {getObjectDump!()}");
        return result;
    }

    static Size? TryGetSize
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode
    )
        => getSize?.Value
            ?? getType?.Value?.OverView.Size
            ?? getCode?.Value?.Size
            ?? (getIsHollow?.Value == true? Size.Zero : null);

    static bool GetIsHollow
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode,
        Func<string>? getObjectDump
    )
    {
        var result = TryGetIsHollow(getIsHollow, getSize, getType, getCode);
        if(result != null)
            return result.Value;
        Tracer.AssertionFailed($"It cannot be obtained if it is hollow for {getObjectDump!()}");
        return false;
    }

    static bool? TryGetIsHollow
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode
    ) =>
        getIsHollow?.Value
        ?? getSize?.Value?.IsZero
        ?? getType?.Value?.OverView.IsHollow
        ?? getCode?.Value?.IsEmpty;

    static Closures? GetClosures
    (
        ValueCache<bool?>? getIsHollow,
        ValueCache<Size?>? getSize,
        ValueCache<TypeBase?>? getType,
        ValueCache<CodeBase?>? getCode,
        ValueCache<Closures?>? getClosures
    )
    {
        if(getClosures != null)
            return getClosures.Value;
        if(getCode != null)
            return getCode.Value!.Closures;
        if(TryGetIsHollow(getIsHollow, getSize, getType, getCode) == true)
            return Closures.GetVoid();

        return null;
    }

    public static ResultData CreateInstance
    (
        Category category
        , Func<TypeBase?>? getType
        , Func<CodeBase?>? getCode
        , Func<Closures?>? getClosures = null
        , Func<Size?>? getSize = null
        , Func<bool?>? getIsHollow = null
        , Func<string>? getObjectDump = null
    )
    {
        var isHollow = getIsHollow == null? null : new ValueCache<bool?>(getIsHollow);
        var size = getSize == null? null : new ValueCache<Size?>(getSize);
        var type = getType == null? null : new ValueCache<TypeBase?>(getType);
        var code = getCode == null? null : new ValueCache<CodeBase?>(getCode);
        var closures = getClosures == null? null : new ValueCache<Closures?>(getClosures);

        return new()
        {
            Type = category.HasType()? GetType(isHollow, size, type, code, getObjectDump) : null
            , Code = category.HasCode()? GetCode(isHollow, size, type, code, getObjectDump) : null
            , Size = category.HasSize()? GetSize(isHollow, size, type, code, getObjectDump) : null
            , Closures = category.HasClosures()? GetClosures(isHollow, size, type, code, closures) : null
            , IsHollow = category.HasIsHollow()? GetIsHollow(isHollow, size, type, code, getObjectDump) : null
        };
    }
}