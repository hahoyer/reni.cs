using hw.DebugFormatter;
using hw.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Validation;

namespace Reni.Code;

public sealed class CodeContainer : DumpableObject, ValueCache.IContainer
{
    [Node]
    readonly FunctionCache<int, FunctionContainer> FunctionsCache;

    readonly ValueCache<string> CSharpStringCache;

    [Node]
    readonly ValueCache<Container> MainCache;

    readonly string ModuleName;
    readonly Root Root;

    internal IEnumerable<Issue> Issues
        => Main
            .Issues
            .Plus(Functions.SelectMany(f => f.Value.Issues));

    internal Container Main => MainCache.Value;

    [DisableDump]
    internal string CSharpString => CSharpStringCache.Value;

    FunctionCache<int, FunctionContainer> Functions
    {
        get
        {
            for(var i = 0; i < Root.FunctionCount; i++)
                FunctionsCache.IsValid(i, true);
            return FunctionsCache;
        }
    }

    internal CodeContainer(ValueSyntax syntax, Root root, string moduleName, string description)
    {
        ModuleName = moduleName;
        Root = root;
        MainCache = new(() => root.GetMainContainer(syntax, description));
        CSharpStringCache = new(GetCSharpStringForCache);
        FunctionsCache = new(Root.GetFunctionContainer);
    }

    ValueCache ValueCache.IContainer.Cache => new();

    public override string DumpData()
    {
        var result = "main\n" + Main.Dump() + "\n";
        for(var i = 0; i < Root.FunctionCount; i++)
            result += "function index=" + i + "\n" + FunctionsCache[i].Dump() + "\n";
        return result;
    }

    internal void Execute(IExecutionContext context, ITraceCollector traceCollector)
        => Main.Data.Execute(context, traceCollector);

    internal CodeBase Function(FunctionId functionId)
    {
        var item = FunctionsCache[functionId.Index];
        var container = functionId.IsGetter? item.Getter : item.Setter;
        return container.Data;
    }

    string GetCSharpStringForCache()
        => ModuleName.CreateCSharpString(Main, Functions);
}