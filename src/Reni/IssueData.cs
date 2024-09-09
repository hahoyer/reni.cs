﻿using hw.DebugFormatter;
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
        internal static readonly IssueType Instance = new();

        int IContextReference.Order => default;

        [DisableDump]
        internal override Root Root => null!;

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
        internal override CodeBase Concat(FiberItem subsequentElement) => this;
    }

    public Issue[] Issues;
    public Category Category;

    public bool? IsHollow => HasIssue && Category.HasIsHollow()? IssueCode.Instance.IsHollow : null;
    public CodeBase Code => HasIssue && Category.HasCode()? IssueCode.Instance : null;
    public Size Size => HasIssue && Category.HasSize()? IssueCode.Instance.Size : null;
    public TypeBase Type => HasIssue && Category.HasType()? IssueType.Instance : null;
    public Closures Closure => HasIssue && Category.HasClosures()? IssueCode.Instance.Closures : null;

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

    public static void AssertValid(Issue[] issues)
    {
        if(issues == null)
            return;
        for(var first = 0; first < issues.Length-1; first++)
        for(var other = first + 1; other < issues.Length; other++)
        {
            (issues[first] != issues[other]).Assert();
        }
    }
    public void AssertValid() => AssertValid(Issues);
}