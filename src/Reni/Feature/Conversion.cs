using hw.DebugFormatter;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature;

sealed class Conversion : DumpableObject, IConversion
{
    static int NextObjectId;

    Func<Category, Result> Function { get; }

    [EnableDump]
    TypeBase Source { get; }

    [EnableDump]
    TypeBase Destination => Function(Category.Type).Type;

    internal Conversion(Func<Category, Result> function, TypeBase source)
        : base(NextObjectId++)
    {
        Function = function;
        Source = source;
        (Source != null).Assert();
        StopByObjectIds();
    }

    Result IConversion.Execute(Category category) => Function(category);
    TypeBase IConversion.Source => Source;

    protected override string GetNodeDump()
        => Source.DumpPrintText
            + "-->"
            + (Destination?.DumpPrintText ?? "<unknown>")
            + " MethodName="
            + Function.Method.Name;
}