using Reni.Struct;
using Reni.Validation;

namespace Reni.Code;

sealed class Container : DumpableObject
{
    static int NextObjectId;

    [Node]
    [DisableDump]
    public static Container UnexpectedVisitOfPending { get; }
        = new("UnexpectedVisitOfPending");

    [Node]
    internal readonly FunctionId FunctionId;

    [Node]
    internal readonly Issue[] Issues;

    [Node]
    [EnableDump]
    internal CodeBase Data { get; }

    [Node]
    [EnableDump]
    internal string Description { get; }

    bool HasIssues => Issues?.Any() ?? false;
    bool HasCode => Data != null;

    internal Container(CodeBase data, Issue[] issues, string description, FunctionId functionId = null)
        : base(NextObjectId++)
    {
        Description = description;
        Issues = issues;
        if(!HasIssues)
            Data = data;
        FunctionId = functionId;

        (HasCode != HasIssues).Assert();
    }

    Container(string errorText) => Description = errorText;

    public string GetCSharpStatements(int indent)
    {
        var generator = new CSharpGenerator(Data?.TemporarySize.SaveByteCount ?? 0);
        Data?.Visit(generator);
        return generator.Data.Indent(indent);
    }
}