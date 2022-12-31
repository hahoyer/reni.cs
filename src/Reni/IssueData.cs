using System;
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
    internal sealed class IssueType : TypeBase, IContextReference
    {
        [DisableDump]
        internal override Root Root { get; }

        public IssueType(Root root) => Root = root;
        int IContextReference.Order => default;

        [DisableDump]
        internal override bool IsHollow => true;

        protected override Size GetSize() => Size.Zero;
    }

    internal sealed class IssueCode : CodeBase
    {
        internal static readonly IssueCode Instance = new();

        IssueCode()
            : base(0) { }

        protected override Size GetSize() => Size.Zero;
        internal override CodeBase Add(FiberItem subsequentElement) => this;
    }

    public Issue[] Issues;
    public Category Category;

    internal readonly Func<Root> Root;

    public bool? IsHollow => HasIssue && Category.HasIsHollow()? IssueCode.Instance.IsHollow : null;
    public CodeBase Code => HasIssue && Category.HasCode()? IssueCode.Instance : null;
    public Size Size => HasIssue && Category.HasSize()? IssueCode.Instance.Size : null;
    public TypeBase Type => HasIssue && Category.HasType()? Root().IssueType : null;
    public Closures Closure => HasIssue && Category.HasClosures()? IssueCode.Instance.Closures : null;

    public bool HasIssue => Issues?.Any() ?? false;
    public IssueData(Func<Root> root) => Root = root;

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