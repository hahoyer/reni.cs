using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Struct;

sealed class AccessFeature
    : DumpableObject
        , IImplementation
        , IValue
        , IConversion
        , ResultCache.IResultProvider
{
    static int NextObjectId;

    [EnableDump]
    public CompoundView View { get; }

    [EnableDump]
    public int Position { get; }

    ValueCache<IFunction> FunctionFeature { get; }

    ValueSyntax Statement => View
        .Compound
        .Syntax
        .PureStatements[Position];

    internal AccessFeature(CompoundView compoundView, int position)
        : base(NextObjectId++)
    {
        View = compoundView;
        Position = position;
        FunctionFeature = new(ObtainFunctionFeature);
        StopByObjectIds();
    }

    Result IConversion.Execute(Category category)
        => GetResult(category).ConvertToConverter(View.Type.Pointer);

    TypeBase IConversion.Source => View.Type.Pointer;

    IFunction IEvalImplementation.Function => FunctionFeature.Value;

    IValue IEvalImplementation.Value
    {
        get
        {
            var function = FunctionFeature.Value;
            if(function != null && function.IsImplicit)
                return null;
            return this;
        }
    }

    IMeta IMetaImplementation.Function
        => (Statement as FunctionSyntax)?.MetaFunctionFeature(View);

    Result ResultCache.IResultProvider.Execute(Category category) => GetResult(category);

    Result IValue.Execute(Category category) => GetResult(category);

    Result GetResult(Category category) => View.AccessViaObject(category, Position);

    IFunction ObtainFunctionFeature()
    {
        if(Statement is FunctionSyntax functionSyntax)
            return functionSyntax.FunctionFeature(View);

        var valueType = View.ValueType(Position);
        StopByObjectIds();
        return ((IEvalImplementation)valueType.CheckedFeature)?.Function;
    }
}