using hw.Scanner;

namespace Reni.TokenClasses.Whitespace;

public sealed class Edit : DumpableObject
{
    [DisableDump]
    internal readonly SourcePart Remove;

    [EnableDump(Order = 2)]
    internal readonly string Insert;

    internal readonly string? Flag;

    [EnableDump(Order = 1)]
    string Position => Remove.NodeDump;

    [DisableDump]
    [PublicAPI]
    internal bool IsEmpty => Insert == "" && Remove.Length == 0;

    internal Edit(SourcePart remove, string insert, string flag)
    {
        Remove = remove;
        Insert = insert;
        Flag = flag;
    }

    protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

    static Edit? Create1(SourcePart sourcePart, string insert)
    {
        var remove = sourcePart.Id;
        if(remove == insert)
            return null;

        if(remove == "")
            return new(sourcePart, insert, "insertAll");

        if(insert == "")
            return new(sourcePart, insert, "removeAll");

        if(remove.Length > insert.Length)
        {
            if(remove.StartsWith(insert))
                return new((sourcePart.Start + insert.Length).Span(sourcePart.End), "", "removeStart");

            if(remove.EndsWith(insert))
                return new(sourcePart.Start.Span(sourcePart.End - insert.Length), "", "removeEnd");

            return new(sourcePart, insert, "replaceAll<");
        }

        if(insert.StartsWith(remove))
            return new(sourcePart.End.Span(0), insert[remove.Length..], "insertEnd");

        if(insert.EndsWith(remove))
            return new(sourcePart.Start.Span(0), insert[..^remove.Length]
                , "insertStart");

        return new(sourcePart, insert, "replaceAll>");
    }

    internal static IEnumerable<Edit> Create(SourcePart sourcePart, string insert)
    {
        var result = Create1(sourcePart, insert);
        if(result != null)
            yield return result;
    }
}