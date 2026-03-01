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

    int IConversion.Weight => 1;

    internal Conversion(Func<Category, Result> function, TypeBase source)
        : base(NextObjectId++)
    {
        Function = function;
        Source = source;
        StopByObjectIds();
    }

    Result IConversion.Execute(Category category) => Function(category);
    TypeBase IConversion.Source => Source;
}