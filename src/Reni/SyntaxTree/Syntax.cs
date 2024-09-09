using System.Diagnostics;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree;

/// <summary>
///     Static syntax items
/// </summary>
abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer, IItem
{
    internal abstract class NoChildren : Syntax
    {
        protected NoChildren(Anchor anchor)
            : base(anchor) { }

        [DisableDump]
        protected sealed override int DirectChildCount => 0;

        protected sealed override Syntax GetDirectChild(int index)
            => throw new($"Unexpected call: {nameof(GetDirectChild)}({index})");
    }

    internal sealed class IssueSyntax : NoChildren
    {
        readonly Issue Issue;

        internal IssueSyntax(Issue issue, Anchor anchor)
            : base(anchor)
            => Issue = issue;

        protected override IEnumerable<Issue> GetIssues() => T(Issue);
    }

    internal class Level
    {
        public bool IsCorrectMapping;
        public bool IsCorrectOrder;
    }

    internal readonly Anchor Anchor;

    [EnableDumpExcept(null)]
    internal Issue[] Issues => this.CachedValue(() => GetIssues()?.ToArray() ?? new Issue[0]);

    internal BinaryTree MainAnchor => Anchor.Main;

    [EnableDump]
    [EnableDumpExcept(null)]
    internal string Position => Anchor.SourceParts.DumpSource();

    [DisableDump]
    internal IEnumerable<Syntax> Children => this.GetNodesFromLeftToRight();

    internal Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

    public BinaryTree LeftMostAnchor
    {
        get
        {
            var main = Anchor.Items.FirstOrDefault();
            var mainPosition = main?.Token;
            var child = DirectChildren.FirstOrDefault()?.LeftMostAnchor;
            var childPosition = child?.Token;
            return mainPosition != null && (childPosition == null || mainPosition < childPosition)
                ? main
                : child;
        }
    }

    public BinaryTree RightMostAnchor
    {
        get
        {
            var main = Anchor.Items.LastOrDefault();
            var mainPosition = main?.Token;
            var child = DirectChildren.LastOrDefault()?.RightMostAnchor;
            var childPosition = child?.Token;
            return mainPosition != null && (childPosition == null || mainPosition > childPosition)
                ? main
                : child;
        }
    }

    public SourcePart ChildSourcePart => LeftMostAnchor.SourcePart.Start.Span(RightMostAnchor.SourcePart.End);
    internal SourcePart[] Anchors => Anchor.SourceParts;

    [DisableDump]
    internal Syntax Parent => MainAnchor.Parent?.Syntax;

    protected Syntax(Anchor anchor, int? objectId = null)
        : base(objectId)
    {
        Anchor = anchor;
        if(!Anchor.IsEmpty)
            Anchor.SourceParts.AssertIsNotNull();
        Anchor.SetSyntax(this);
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    Anchor IItem.Anchor => Anchor;
    Syntax[] IItem.DirectChildren => DirectChildren;
    BinaryTree IItem.SpecialAnchor => null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int ITree<Syntax>.DirectChildCount => DirectChildCount;

    Syntax ITree<Syntax>.GetDirectChild(int index)
        => index < 0 || index >= DirectChildCount? null : DirectChildren[index];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int ITree<Syntax>.LeftDirectChildCount => 0;

    [DisableDump]
    protected abstract int DirectChildCount { get; }

    protected abstract Syntax GetDirectChild(int index);

    protected virtual IEnumerable<Issue> GetIssues() => null;

    internal virtual Result<ValueSyntax> ToValueSyntax()
    {
        if(GetType().Is<ValueSyntax>())
            return (ValueSyntax)this;

        NotImplementedMethod();
        return default;
    }

    internal virtual Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target = null)
    {
        NotImplementedMethod(target);
        return default;
    }


    internal virtual void AssertValid(Level level = null, BinaryTree target = null)
    {
        level ??= new();

        foreach(var node in DirectChildren.Where(node => node != this))
            node?.AssertValid(level, target);
    }

    internal Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target = null)
        => ToCompoundSyntaxHandler(target);

    internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
        => this.GetNodesFromLeftToRight().SelectMany(node => node.CheckedItemsAsLongAs(condition));

    internal IEnumerable<SourcePart> GetParserLevelGroup(int index)
        => Anchor.Items
            .Where(item => item.TokenClass.IsBelongingTo(Anchor.Items[index].TokenClass))
            .Select(item => item.Token);
}