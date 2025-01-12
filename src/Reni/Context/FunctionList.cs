using Reni.Code;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Context;

sealed class FunctionList : DumpableObject
{
    [Node]
    readonly
        FunctionCache<FunctionSyntax, FunctionCache<CompoundView, FunctionCache<TypeBase, int>>>
        Dictionary;

    [Node]
    readonly List<FunctionType> List = new();

    internal int Count => List.Count;

    public FunctionList() => Dictionary = new(
        syntax => new(
            structure => new(-1, args => CreateFunctionInstance(args, syntax, structure))));

    internal FunctionType Find(FunctionSyntax syntax, CompoundView compoundView, TypeBase argsType)
    {
        var index = Dictionary[syntax][compoundView][argsType];
        return List[index];
    }

    internal IEnumerable<FunctionType> Find(FunctionSyntax syntax, CompoundView compoundView)
        => Dictionary[syntax][compoundView].Select(item => List[item.Value]);

    internal FunctionContainer Container(int index) => List[index].Container;
    internal FunctionType Item(int index) => List[index];

    int CreateFunctionInstance(TypeBase args, FunctionSyntax syntax, CompoundView compoundView)
    {
        var index = List.Count;
        var f = new FunctionType(index, syntax, compoundView, args);
        List.Add(f);
        return index;
    }
}