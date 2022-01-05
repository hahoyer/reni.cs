using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree;

public sealed class Anchor : DumpableObject, ValueCache.IContainer
{
    public readonly BinaryTree[] Items;

    Anchor(params BinaryTree[] items)
    {
        Items = items
            .Where(item => item != null)
            .Distinct()
            .OrderBy(item => item.Token.Position)
            .ToArray();

        Items.Any().Assert();
        Main.AssertIsNotNull();
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

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

    internal Anchor GetLeftOf(BinaryTree target) => GetLeftOf(target.Token.Start);
    internal Anchor GetRightOf(BinaryTree target) => GetRightOf(target.Token.End);

    [PublicAPI]
    internal Anchor GetLeftOf(SourcePosition position)
        => new(Items.Where(item => item.Token < position).ToArray());

    [PublicAPI]
    internal Anchor GetRightOf(SourcePosition position)
        => new(Items.Where(item => position < item.Token).ToArray());

    internal static Anchor Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
        => new(leftAnchor, rightAnchor);

    internal static Anchor Create(BinaryTree leftAnchor)
        => new(leftAnchor.AssertNotNull());

    internal static Anchor Create(params BinaryTree[] items) => new(items);

    internal static Anchor CheckedCreate(params BinaryTree[] items) 
        => items == null || items.Length == 0? null : new(items);

    internal Anchor Combine(Anchor other) => Combine(other?.Items);

    internal Anchor Combine(BinaryTree[] other)
    {
        if(other == null || !other.Any())
            return this;
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

    public static Anchor CreateAll(BinaryTree target)
        => Create(target.GetNodesFromLeftToRight().ToArray());

    public static Anchor operator +(Anchor left, Anchor right)
        => left == null
            ? right
            : right == null
                ? left
                : left.Combine(right);
}