using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.TokenClasses.Whitespace;
using Reni.Validation;

namespace ReniUI.Classification;

public abstract class Item : DumpableObject
{
    public sealed class Trimmed : DumpableObject
    {
        [DisableDump]
        internal readonly Item Item;

        [DisableDump]
        internal readonly SourcePart SourcePart;

        internal Trimmed(Item item, SourcePart sourcePart)
        {
            Item = item;
            SourcePart = sourcePart.Intersect(item.SourcePart) ?? item.SourcePart.Start.Span(0);
        }

        public IEnumerable<char> GetCharArray()
            => SourcePart
                .Id
                .ToCharArray();
    }

    internal BinaryTree Anchor { get; }

    internal TokenClass TokenClass => Anchor.TokenClass as TokenClass;

    public char TypeCharacter
    {
        get
        {
            if(IsComment)
                return 'c';
            if(IsSpace)
                return 'w';
            if(IsLineEnd)
                return '$';
            if(IsNumber)
                return 'n';
            if(IsText)
                return 't';
            if(IsKeyword)
                return 'k';
            if(IsIdentifier)
                return 'i';
            if(IsBrace)
                return 'b';
            if(IsError)
                return 'e';
            NotImplementedMethod();
            return '?';
        }
    }

    internal string[] Types => GetTypes().ToArray();
    string TypeList => GetTypes().Stringify(",");

    public int StartPosition => SourcePart.Position;
    public int EndPosition => SourcePart.EndPosition;

    [DisableDump]
    internal Reni.SyntaxTree.Syntax Master => Anchor.Syntax;

    [DisableDump]
    public int Index => Master.Anchor.Items.IndexWhere(item => item == Anchor).AssertValue();

    public IEnumerable<Item> Belonging
    {
        get
        {
            if(IsBraceLike)
                return Anchor.ParserLevelGroup.Select(item => new Syntax(item));
            return new Item[0];
        }
    }

    protected Item(BinaryTree anchor) => Anchor = anchor;

    [DisableDump]
    public virtual SourcePart SourcePart => Anchor.Token;

    [DisableDump]
    public virtual bool IsKeyword => false;

    [DisableDump]
    public virtual bool IsIdentifier => false;

    [DisableDump]
    public virtual bool IsText => false;

    [DisableDump]
    public virtual bool IsNumber => false;

    [DisableDump]
    public virtual bool IsComment => false;

    [DisableDump]
    public virtual bool IsSpace => false;

    [DisableDump]
    public virtual bool IsBraceLike => false;

    [DisableDump]
    public virtual bool IsLineEnd => false;

    [DisableDump]
    public virtual bool IsError => false;

    [DisableDump]
    public virtual bool IsBrace => false;

    [DisableDump]
    public virtual bool IsPunctuation => false;

    [DisableDump]
    public virtual bool IsWhiteSpace => false;


    [DisableDump]
    public virtual string State => "";

    [DisableDump]
    public virtual IEnumerable<SourcePart> ParserLevelGroup => null;

    [DisableDump]
    public virtual Issue[] Issues => null;

    internal virtual IWhitespaceItem GetItem<TItemType>()
        where TItemType : IItemType
        => null;

    public override bool Equals(object obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        if(obj.GetType() != GetType())
            return false;
        return Equals((Item)obj);
    }

    public override int GetHashCode() => SourcePart.GetHashCode();

    IEnumerable<string> GetTypes()
    {
        if(IsError)
            yield return "error";
        if(IsComment)
            yield return "comment";
        if(IsSpace)
            yield return "space";
        if(IsLineEnd)
            yield return "line-end";
        if(IsNumber)
            yield return "number";
        if(IsText)
            yield return "text";
        if(IsKeyword)
            yield return "keyword";
        if(IsIdentifier)
            yield return "identifier";
        if(IsBrace)
            yield return "brace";
        if(IsBraceLike)
            yield return "brace-like";
    }

    internal Trimmed TrimLine(SourcePart span) => new(this, span);

    bool Equals(Item other) => SourcePart == other.SourcePart;

    internal static Item LocateByPosition(BinaryTree target, SourcePosition offset)
    {
        var result = target.FindItem(offset);
        result.AssertIsNotNull();
        var item = result.WhiteSpaces.LocateItem(offset);
        if(item == null)
            return new Syntax(result);
        return new WhiteSpaceItem(item, result);
    }

    internal static BinaryTree Locate(BinaryTree target, SourcePart span)
    {
        var start = LocateByPosition(target, span.Start);
        var end = LocateByPosition(target, span.End);
        var result = start.Anchor.CommonRoot(end.Anchor);
        result.AssertIsNotNull();
        return result;
    }

    internal static Item GetRightNeighbor(Helper.Syntax target, int current)
    {
        NotImplementedFunction(target, current);
        return default;
    }

    internal string ShortDump()
    {
        var start = SourcePart.Start.TextPosition;
        return
            $"{start.LineNumber}/{start.ColumnNumber1 - 1}: {TypeList}: {SourcePart.Id.Quote()}";
    }
}