using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace Reni.Basics;

[Dump("Dump")]
sealed class Category : DumpableObject, IEquatable<Category>
{
    static readonly Category[] Cache = new Category[32];
    public readonly bool HasCode;
    public readonly bool HasType;
    public readonly bool HasClosures;
    public readonly bool HasSize;
    public readonly bool HasIsHollow;

    [DebuggerHidden]
    public static Category Size => CreateCategory(size: true);

    [DebuggerHidden]
    public static Category Type => CreateCategory(type: true);

    [DebuggerHidden]
    public static Category Code => CreateCategory(code: true);

    [DebuggerHidden]
    public static Category Closures => CreateCategory(closures: true);

    [DebuggerHidden]
    public static Category IsHollow => CreateCategory(true);

    [DebuggerHidden]
    public static Category None => CreateCategory();

    [DebuggerHidden]
    public static Category All => CreateCategory(true, true, true, true, true);

    public bool IsNone => !HasAny;

    public bool HasAny => HasCode || HasType || HasClosures || HasSize || HasIsHollow;

    [DebuggerHidden]
    [DisableDump]
    public Category WithHollow => this | IsHollow;

    [DebuggerHidden]
    [DisableDump]
    public Category WithSize => this | Size;

    [DebuggerHidden]
    [DisableDump]
    public Category WithType => this | Type;

    [DebuggerHidden]
    [DisableDump]
    public Category WithCode => this | Code;

    [DebuggerHidden]
    [DisableDump]
    public Category WithClosures => this | Closures;

    public Category Replenished
    {
        get
        {
            var result = this;
            if(result.HasCode)
            {
                result |= Size;
                result |= Closures;
            }

            if(result.HasSize)
                result |= IsHollow;
            return result;
        }
    }

    internal Category ReplaceArged
    {
        get
        {
            var result = None;
            if(HasCode)
                result |= Type | Code;
            if(HasClosures)
                result |= Closures;
            return result;
        }
    }

    internal Category FunctionCall
    {
        get
        {
            var result = this - Closures - Code;
            return HasCode? result.WithSize : result;
        }
    }

    Category(bool isHollow, bool size, bool type, bool code, bool closures)
        : base(null)
    {
        HasCode = code;
        HasType = type;
        HasClosures = closures;
        HasIsHollow = isHollow;
        HasSize = size;
    }

    public bool Equals(Category obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        return
            obj.HasCode.Equals(HasCode) &&
            obj.HasType.Equals(HasType) &&
            obj.HasClosures.Equals(HasClosures) &&
            obj.HasSize.Equals(HasSize) &&
            obj.HasIsHollow.Equals(HasIsHollow)
            ;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var result = HasCode.GetHashCode();
            result = (result * 397) ^ HasType.GetHashCode();
            result = (result * 397) ^ HasClosures.GetHashCode();
            result = (result * 397) ^ HasSize.GetHashCode();
            result = (result * 397) ^ HasIsHollow.GetHashCode();
            return result;
        }
    }

    protected override string Dump(bool isRecursion) => NodeDump;

    protected override string GetNodeDump()
    {
        var result = "";
        if(HasIsHollow)
            result += ".IsHollow.";
        if(HasSize)
            result += ".Size.";
        if(HasType)
            result += ".Type.";
        if(HasClosures)
            result += ".Closures.";
        if(HasCode)
            result += ".Code.";
        result = result.Replace("..", ",").Replace(".", "");
        if(result == "")
            return "none";
        return result;
    }

    public override bool Equals(object obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        if(obj.GetType() != typeof(Category))
            return false;
        return Equals((Category)obj);
    }

    [DebuggerHidden]
    internal static Category CreateCategory
        (bool isHollow = false, bool size = false, bool type = false, bool code = false, bool closures = false)
    {
        var result = Cache[IndexFromBool(isHollow, size, type, code, closures)];
        if(result != null)
            return result;
        return Cache[IndexFromBool(isHollow, size, type, code, closures)] = new(isHollow, size, type, code, closures);
    }

    static int IndexFromBool(params bool[] data)
        => data.Aggregate(0, (c, n) => c * 2 + (n? 1 : 0));

    [DebuggerHidden]
    public static Category operator |(Category x, Category y) => CreateCategory
    (
        x.HasIsHollow || y.HasIsHollow,
        x.HasSize || y.HasSize,
        x.HasType || y.HasType,
        x.HasCode || y.HasCode,
        x.HasClosures || y.HasClosures);

    [DebuggerHidden]
    public static Category operator &(Category x, Category y) => CreateCategory
    (
        x.HasIsHollow && y.HasIsHollow,
        x.HasSize && y.HasSize,
        x.HasType && y.HasType,
        x.HasCode && y.HasCode,
        x.HasClosures && y.HasClosures);

    public bool IsEqual(Category x) =>
        HasCode == x.HasCode &&
        HasClosures == x.HasClosures &&
        HasSize == x.HasSize &&
        HasType == x.HasType &&
        HasIsHollow == x.HasIsHollow;

    [PublicAPI]
    bool IsLessThan(Category x) =>
        (!HasCode && x.HasCode) ||
        (!HasClosures && x.HasClosures) ||
        (!HasSize && x.HasSize) ||
        (!HasType && x.HasType) ||
        (!HasIsHollow && x.HasIsHollow);

    bool IsLessThanOrEqual(Category x)
    {
        if(HasCode && !x.HasCode)
            return false;
        if(HasClosures && !x.HasClosures)
            return false;
        if(HasSize && !x.HasSize)
            return false;
        if(HasType && !x.HasType)
            return false;
        if(HasIsHollow && !x.HasIsHollow)
            return false;
        return true;
    }

    [DebuggerHidden]
    public static Category operator -(Category x, Category y) => CreateCategory
    (
        x.HasIsHollow && !y.HasIsHollow,
        x.HasSize && !y.HasSize,
        x.HasType && !y.HasType,
        x.HasCode && !y.HasCode,
        x.HasClosures && !y.HasClosures);

    public static bool operator <(Category left, Category right) => left != right && left <= right;

    public static bool operator <=(Category left, Category right) => left.IsLessThanOrEqual(right);

    public static bool operator >=(Category left, Category right) => right <= left;
    public static bool operator >(Category left, Category right) => right < left;
    public static bool operator ==(Category left, Category right) => Equals(left, right);
    public static bool operator !=(Category left, Category right) => !Equals(left, right);
}