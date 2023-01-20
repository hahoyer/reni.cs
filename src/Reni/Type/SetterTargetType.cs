using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type;

abstract class SetterTargetType
    : TypeBase, IProxyType, IConversion, IValue, IReference, ISymbolProvider<ReassignToken>
{
    readonly int Order;

    protected SetterTargetType() => Order = Closures.NextOrder++;

    int IContextReference.Order => Order;

    Result IConversion.Execute(Category category)
        => GetterResult(category).ConvertToConverter(this);

    TypeBase IConversion.Source => this;

    IConversion IProxyType.Converter => this;
    IConversion IReference.Converter => this;
    bool IReference.IsWeak => true;

    IImplementation ISymbolProvider<ReassignToken>.Feature(ReassignToken tokenClass)
        => IsMutable? Feature.Extension.FunctionFeature(ReassignResult) : null;

    Result IValue.Execute(Category category) => GetterResult(category);

    [EnableDumpExcept(false)]
    protected abstract bool IsMutable { get; }

    [DisableDump]
    internal abstract TypeBase ValueType { get; }

    protected abstract Result SetterResult(Category category);
    protected abstract Result GetterResult(Category category);

    internal virtual Result DestinationResult(Category category) => GetArgumentResult(category);

    [DisableDump]
    internal override bool IsAligningPossible => false;

    [DisableDump]
    internal override bool IsPointerPossible => false;

    [DisableDump]
    internal override Root Root => ValueType.Root;

    [DisableDump]
    internal override TypeBase TypeForTypeOperator => ValueType.TypeForTypeOperator;

    [DisableDump]
    internal override TypeBase ElementTypeForReference => ValueType.ElementTypeForReference;

    protected override IEnumerable<IConversion> RawSymmetricConversions
    {
        get { yield break; }
    }

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
    {
        get { yield return Feature.Extension.Conversion(GetterResult); }
    }

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
                .GetConversion(category | Category.Type, ValueType.ForcedPointer);
            Dump("sourceResult", sourceResult);
            BreakExecution();

            var destinationResult = DestinationResult(category | Category.Type)
                .ReplaceArg(GetResult(category | Category.Type, this));
            Dump("destinationResult", destinationResult);
            BreakExecution();

            var resultForArg = destinationResult + sourceResult;
            Dump("resultForArg", resultForArg);

            var result = SetterResult(category);
            Dump("result", result);
            BreakExecution();

            return ReturnMethodDump(result.ReplaceArg(resultForArg));
        }
        finally
        {
            EndMethodDump();
        }
    }
}