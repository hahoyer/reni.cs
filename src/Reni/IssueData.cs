using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;
using Reni.Validation;

namespace Reni;

sealed class IssueData : DumpableObject
{
    internal sealed class Default : Singleton<Default, DumpableObject>
    {
        internal sealed class IssueType : TypeBase, IContextReference
        {
            int IContextReference.Order => default;
            internal override Root Root => default;
            protected override Size GetSize() => Size.Zero;
            internal override bool IsHollow => true;
        }

        internal sealed class IssueCode : CodeBase
        {
            public IssueCode()
                : base(0) { }

            protected override Size GetSize() => Size.Zero;
            internal override CodeBase Add(FiberItem subsequentElement) => this;
        }

        internal readonly ResultData Value;

        public Default()
        {
            var type = new IssueType();
            var code = new IssueCode();
            Value = new(Category.All, () => type, () => code);
        }
    }

    public Issue[] Issues;
    public Category Category;

    public bool? IsHollow => HasIssue && Category.HasIsHollow()? Default.Instance.Value.IsHollow : null;
    public CodeBase Code => HasIssue && Category.HasCode()? Default.Instance.Value.Code : null;
    public Size Size => HasIssue && Category.HasSize()? Default.Instance.Value.Size : null;
    public TypeBase Type => HasIssue && Category.HasType()? Default.Instance.Value.Type : null;
    public Closures Closure => HasIssue && Category.HasClosures()? Default.Instance.Value.Closures : null;

    public bool HasIssue => Issues?.Any() ?? false;

    public void Set(Category category, bool value = true)
    {
        if(!HasIssue)
            return;

        if(value)
            Category |= category;
        else
            Category &= ~category;
    }
}