using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree;

sealed class Anchor : DumpableObject, ValueCache.IContainer
{
    internal readonly BinaryTree[] Items;
    readonly string ReasonForEmptiness;

    [DisableDump]
    internal SourcePart[] SourceParts => Items.SourceParts();

    [DisableDump]
    internal SourcePart SourcePart => SourceParts.Combine();

    [DisableDump]
    public IEnumerable<Issue> Issues => Items.SelectMany(node => node.AllIssues);

    [DisableDump]
    public bool IsEmpty => !Items.Any();

    [DisableDump]
    internal BinaryTree Main => this.CachedValue(GetMain);

    Anchor(params BinaryTree?[] items)
    {
        Items = items
            .Where(item => item != null)
            .Distinct()
            .OrderBy(item => item.Token.Position)
            .ToArray();

        Items.Any().Assert();
        Main.AssertIsNotNull();
    }

    Anchor(string reasonForEmptiness)
    {
        Items = new BinaryTree[0];
        ReasonForEmptiness = reasonForEmptiness;
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override string GetNodeDump()
    {
        var itemDump = ReasonForEmptiness ?? $"[{Items.Length}]";
        return base.GetNodeDump() + itemDump;
    }

    internal Anchor GetLeftOf(BinaryTree? target) => GetLeftOf(target.Token.Start);
    internal Anchor GetRightOf(BinaryTree? target) => GetRightOf(target.Token.End);

    [PublicAPI]
    internal Anchor GetLeftOf(SourcePosition position)
        => new(Items.Where(item => item.Token < position).ToArray());

    [PublicAPI]
    internal Anchor GetRightOf(SourcePosition position)
        => new(Items.Where(item => position < item.Token).ToArray());

    internal static Anchor Create(string reasonForEmptiness)
        => new(reasonForEmptiness);

    internal static Anchor Create(BinaryTree? leftAnchor, BinaryTree? rightAnchor)
        => new(leftAnchor, rightAnchor);

    internal static Anchor Create(BinaryTree? leftAnchor)
        => new(leftAnchor.AssertNotNull());

    internal static Anchor Create(params BinaryTree?[] items) => new(items);

    internal static Anchor CheckedCreate(params BinaryTree?[] items)
        => items == null || items.Length == 0? null : new(items);

    internal Anchor Combine(Anchor other, bool check = false)
    {
        if(check)
            other?.ReasonForEmptiness.ExpectIsNull(() => (SourcePart, "Cannot combine with empty anchor."));
        return Combine(other?.Items);
    }

    internal Anchor Combine(BinaryTree[] other)
    {
        if(other == null || !other.Any())
            return this;
        ReasonForEmptiness.AssertIsNull(() => "Cannot combine empty anchor.");
        return new(other.Concat(Items).ToArray());
    }

    BinaryTree GetMain()
        => Items
            .Single(node => Items.All(parent => !node.HasAsParent(parent)));

    public void SetSyntax(Syntax syntax)
    {
        foreach(var item in Items)
            item.SetSyntax(syntax);
    }

    public static Anchor CreateAll(BinaryTree? target)
        => Create(target.GetNodesFromLeftToRight().ToArray());

    public static Anchor operator +(Anchor left, Anchor right)
        => left == null? right :
            right == null? left : left.Combine(right, true);
}