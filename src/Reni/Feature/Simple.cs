using Reni.Basics;
using Reni.Type;

namespace Reni.Feature;

sealed class Value
    : DumpableObject
        , IImplementation
        , IValue
{
    static int NextObjectId;

    [EnableDump]
    internal Func<Category, Result> Function { get; }

    TypeBase Source { get; }

    internal Value(Func<Category, Result> function, TypeBase source)
        : base(NextObjectId++)
    {
        Function = function;
        Source = source;
    }

    IFunction? IEvalImplementation.Function => null;
    IValue IEvalImplementation.Value => this;

    IMeta? IMetaImplementation.Function => null;

    Result IValue.Execute(Category category) => Function(category);

    protected override string GetNodeDump()
        => Source.DumpPrintText
            + "-->"
            + Function(Category.Type).Type.DumpPrintText
            + " MethodName="
            + Function.Method.Name;
}