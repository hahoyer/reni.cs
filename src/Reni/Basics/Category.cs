using System;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Reni.Basics;

[Dump("Dump")]
public sealed class Category : DumpableObject
{
    [UnitTest]
    public sealed class CategoryTest : DependenceProvider
    {
        [UnitTest]
        public void TestWithout()
        {
            CreateCategory(CategoryValue.None).Contains(CreateCategory(CategoryValue.None)).Assert();

            (!CreateCategory(CategoryValue.None).Contains(CreateCategory(CategoryValue.Size))).Assert();
            CreateCategory(CategoryValue.Size).Contains(CreateCategory(CategoryValue.None)).Assert();

            (!CreateCategory(CategoryValue.Size | CategoryValue.Code)
                    .Contains(CreateCategory(CategoryValue.Type | CategoryValue.Code)))
                .Assert();
            (!CreateCategory(CategoryValue.Code).Contains(CreateCategory(CategoryValue.Size | CategoryValue.Code)))
                .Assert();
        }
    }

    static readonly Category[] Cache = new Category[32];

    readonly CategoryValue Value;

    [DebuggerHidden]
    public static Category Size => CreateCategory(CategoryValue.Size);

    [DebuggerHidden]
    public static Category Type => CreateCategory(CategoryValue.Type);

    [DebuggerHidden]
    public static Category Code => CreateCategory(CategoryValue.Code);

    [DebuggerHidden]
    public static Category Closures => CreateCategory(CategoryValue.Closures);

    [DebuggerHidden]
    public static Category IsHollow => CreateCategory(CategoryValue.IsHollow);

    [DebuggerHidden]
    public static Category None => CreateCategory(CategoryValue.None);

    [DebuggerHidden]
    public static Category All => CreateCategory(CategoryValue.All);

    public bool IsNone => !HasAny;

    public bool HasAny => HasCode() || HasType() || HasClosures() || HasSize() || HasIsHollow();

    Category(CategoryValue value)
        : base(null)
        => Value = value;

    protected override string Dump(bool isRecursion) => NodeDump;

    protected override string GetNodeDump()
    {
        var result = "";
        if(HasIsHollow())
            result += ".IsHollow.";
        if(HasSize())
            result += ".Size.";
        if(HasType())
            result += ".Type.";
        if(HasClosures())
            result += ".Closures.";
        if(HasCode())
            result += ".Code.";
        result = result.Replace("..", ",").Replace(".", "");
        if(result == "")
            return "none";
        return result;
    }

    public bool HasCode() => (Value & CategoryValue.Code) != CategoryValue.None;
    public bool HasType() => (Value & CategoryValue.Type) != CategoryValue.None;
    public bool HasClosures() => (Value & CategoryValue.Closures) != CategoryValue.None;
    public bool HasSize() => (Value & CategoryValue.Size) != CategoryValue.None;
    public bool HasIsHollow() => (Value & CategoryValue.IsHollow) != CategoryValue.None;

    public Category Replenished()
    {
        var result = this;
        if(result.HasCode())
        {
            result |= Size;
            result |= Closures;
        }

        if(result.HasSize())
            result |= IsHollow;
        return result;
    }

    internal Category FunctionCall()
    {
        var result = HasCode()? Without(Code) | Size : this;
        return result.Without(Closures);
    }

    internal static Category CreateCategory(CategoryValue value)
    {
        var result = Cache[(int)value];
        if(result != null)
            return result;
        return Cache[(int)value] = new(value);
    }

    static int IndexFromBool(params bool[] data)
        => data.Aggregate(0, (c, n) => c * 2 + (n? 1 : 0));

    [DebuggerHidden]
    public static Category operator |(Category x, Category y) => CreateCategory(x.Value | y.Value);

    [DebuggerHidden]
    public static Category operator &(Category x, Category y) => CreateCategory(x.Value & y.Value);

    internal bool Contains(Category x) => (~Value & x.Value) == CategoryValue.None;

    internal Category Without(Category y) => CreateCategory(Value & ~y.Value);
}

[Flags]
enum CategoryValue
{
    None = 0
    , IsHollow = 1
    , Size = 2
    , Type = 4
    , Code = 8
    , Closures = 16
    , Issues = 32
    , All = 31
}