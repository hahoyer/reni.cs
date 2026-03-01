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
    , Semantics = 32
    , All = 31
}

public static class CategoryExtension
{
    [UnitTest]
    public sealed class Test
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

    extension(Category category)
    {
        public bool HasCode => (category & Category.Code) != Category.None;
        public bool HasType => (category & Category.Type) != Category.None;
        public bool HasClosures => (category & Category.Closures) != Category.None;
        public bool HasSize => (category & Category.Size) != Category.None;
        public bool HasIsHollow => (category & Category.IsHollow) != Category.None;

        public string Dump() => $"{category}";

        public Category Replenished
        {
            get
            {
                var result = category;
                if(result.HasCode)
                {
                    result |= Category.Size;
                    result |= Category.Closures;
                }

                if(result.HasSize)
                    result |= Category.IsHollow;
                return result;
            }
        }

        internal Category FunctionCall
        {
            get
            {
                var result = category;
                if(category.HasCode)
                    result = result.Without(Category.Code) | Category.Size;
                return result.Without(Category.Closures);
            }
        }

        internal bool Contains(Category content)
            => (~category & content) == Category.None;

        internal Category Without(Category content)
            => category & ~content;
    }
}