using System;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Reni.Basics;

[Flags]
public enum Category
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

public static class CategoryExtension
{
    [UnitTest]
    public sealed class Test : DependenceProvider
    {
        [UnitTest]
        public void Without()
        {
            Category.None.Contains(Category.None).Assert();

            (!Category.None.Contains(Category.Size)).Assert();
            Category.Size.Contains(Category.None).Assert();

            (!(Category.Size | Category.Code)
                    .Contains(Category.Type | Category.Code))
                .Assert();
            (!Category.Code.Contains(Category.Size | Category.Code))
                .Assert();
        }
    }

    public static bool HasCode(this Category category) => (category & Category.Code) != Category.None;
    public static bool HasType(this Category category) => (category & Category.Type) != Category.None;
    public static bool HasClosures(this Category category) => (category & Category.Closures) != Category.None;
    public static bool HasSize(this Category category) => (category & Category.Size) != Category.None;
    public static bool HasIsHollow(this Category category) => (category & Category.IsHollow) != Category.None;

    public static string Dump(this Category category) => $"{category}";

    public static Category Replenished(this Category category)
    {
        var result = category;
        if(category.HasCode())
        {
            result |= Category.Size;
            result |= Category.Closures;
        }

        if(category.HasSize())
            result |= Category.IsHollow;
        return result;
    }

    internal static Category FunctionCall(this Category category)
    {
        var result = category;
        if(category.HasCode())
            result = result.Without(Category.Code) | Category.Size;
        return result.Without(Category.Closures);
    }

    internal static bool Contains(this Category container, Category content)
        => (~container & content) == Category.None;

    internal static Category Without(this Category container, Category content)
        => container & ~content;
}