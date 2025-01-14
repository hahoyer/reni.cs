using System.Diagnostics.CodeAnalysis;
using hw.Scanner;
using Reni.TokenClasses.Whitespace;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses;

/// <summary>
///     Hierarchical structured whitespace cluster
/// </summary>
sealed class WhiteSpaceItem : DumpableObject, IWhitespaceItem, ISeparatorClass
{
    internal readonly SourcePart SourcePart;

    internal readonly IItemType Type;

    [DisableDump]
    [PublicAPI]
    internal readonly IParent? Parent;

    [EnableDump]
    [field: AllowNull, MaybeNull]
    internal WhiteSpaceItem[] Items => field ??= GetItems();

    [DisableDump]
    internal bool HasStableLineBreak => Type is IStableLineBreak || Items.Any(item => item.HasStableLineBreak);

    bool HasVolatileLineBreak => Type is IVolatileLineBreak || Items.Any(item => item.HasVolatileLineBreak);

    internal bool HasLineBreak
        => HasStableLineBreak || HasVolatileLineBreak;

    internal WhiteSpaceItem(SourcePart sourcePart)
        : this(RootType.Instance, sourcePart, null) { }

    internal WhiteSpaceItem(IItemType type, SourcePart sourcePart, IParent? parent)
    {
        Type = type;
        SourcePart = sourcePart;
        Parent = parent;
    }

    IWhitespaceItem? IParent.GetItem<TItemType>()
        => Type is TItemType? this : Parent?.GetItem<TItemType>();

    IParent? IParent.Parent => Parent;
    IItemType IParent.Type => Type;

    ContactType ISeparatorClass.ContactType => ContactType.Incompatible;

    SourcePart IWhitespaceItem.SourcePart => SourcePart;

    protected override string GetNodeDump() => SourcePart.NodeDump + " " + base.GetNodeDump();

    public bool IsNotEmpty(bool areEmptyLinesPossible)
        => Items.Any(item => item.Type is IComment || (areEmptyLinesPossible && item.Type is IVolatileLineBreak));

    WhiteSpaceItem[] GetItems()
        => (Type as IItemsType)?.GetItems(SourcePart, this).ToArray() ?? [];

    internal WhiteSpaceItem? GetItem(SourcePosition offset)
    {
        (SourcePart.Start <= offset).Assert();

        if(SourcePart.End <= offset)
            return null;

        if(!Items.Any())
            return this;

        return Items
            .Select(item1 => item1.GetItem(offset))
            .FirstOrDefault(item => item != null);
    }

    internal string? FlatFormat(bool areEmptyLinesPossible, SeparatorRequests separatorRequests)
    {
        if(HasStableLineBreak || (areEmptyLinesPossible && HasVolatileLineBreak))
            return null;

        var result = Items
            .Where(item => item.Type is IInline)
            .Select(item => item.SourcePart.Id)
            .Stringify(separatorRequests.Inner? " " : "");
        if(result == "")
            return separatorRequests.Flat? " " : "";
        return (separatorRequests.Head? " " : "") + result + (separatorRequests.Tail? " " : "");
    }
}