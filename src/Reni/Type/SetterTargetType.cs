using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type;

abstract class SetterTargetType
    : TypeBase
        , IProxyType
        , IConversion
        , IValue
        , IReference
        , ISymbolProvider<ReassignToken>
{
    [DisableDump]
    internal override Root Root { get; }

    readonly int Order = Closures.NextOrder++;

    protected SetterTargetType(Root root) => Root = root;

    int IContextReference.Order => Order;

    Result IConversion.Execute(Category category)
        => GetGetterResult(category).ConvertToConverter(this);

    TypeBase IConversion.Source => this;

    IConversion IProxyType.Converter => this;
    IConversion IReference.Converter => this;
    bool IReference.IsWeak => true;

    IImplementation? ISymbolProvider<ReassignToken>.Feature
        => IsMutable? Feature.Extension.FunctionFeature(ReassignResult) : null;

    Result IValue.Execute(Category category) => GetGetterResult(category);

    [EnableDumpExcept(false)]
    protected abstract bool IsMutable { get; }

    [DisableDump]
    internal abstract TypeBase ValueType { get; }

    protected abstract Result GetSetterResult(Category category);
    protected abstract Result GetGetterResult(Category category);

    internal virtual Result DestinationResult(Category category) => GetArgumentResult(category);

    protected override bool GetIsAligningPossible() => false;

    protected override bool GetIsPointerPossible() => false;

    internal override TypeBase GetTypeForTypeOperator() => ValueType.GetTypeForTypeOperator();

    protected override IEnumerable<IConversion> GetSymmetricConversions() { yield break; }

    protected override IEnumerable<IConversion> GetStripConversions() { yield return Feature.Extension.Conversion(GetGetterResult); }

    Result ReassignResult(Category category, TypeBase right)
    {
        if(category == Category.Type)
            return Root.VoidType.GetResult(category);

        var trace = ObjectId == -97 && category.HasCode();
        StartMethodDump(trace, category, right);
        try
        {
            BreakExecution();
            var sourceResult = right
                .GetConversion(category | Category.Type, ValueType.Make.ForcedPointer);
            Dump("sourceResult", sourceResult);
            BreakExecution();

            var destinationResult = DestinationResult(category | Category.Type)
                .ReplaceArguments(GetResult(category | Category.Type, this));
            Dump("destinationResult", destinationResult);
            BreakExecution();

            var resultForArg = (destinationResult + sourceResult)!;
            Dump("resultForArg", resultForArg);

            var result = GetSetterResult(category);
            Dump("result", result);
            BreakExecution();

            return ReturnMethodDump(result.ReplaceArguments(resultForArg));
        }
        finally
        {
            EndMethodDump();
        }
    }
}