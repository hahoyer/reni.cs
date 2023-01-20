using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Struct;

sealed class FunctionBodyType
    : TypeBase
        , IFunction
        , IValue
        , IConversion
        , IImplementation
        , IChild<CompoundView>
        , ISymbolProvider<DumpPrintToken>
{
    sealed class ContextReference : DumpableObject, IContextReference
    {
        readonly int Order;

        [Node]
        readonly FunctionBodyType Parent;

        [EnableDump]
        FunctionSyntax Syntax => Parent.Syntax;

        public ContextReference(FunctionBodyType parent)
            : base(parent.ObjectId)
        {
            Order = Closures.NextOrder++;
            Parent = parent;
            StopByObjectIds(-5);
        }

        int IContextReference.Order => Order;
    }

    [DisableDump]
    [EnableDump]
    [Node]
    internal override CompoundView FindRecentCompoundView { get; }

    [EnableDump]
    [Node]
    internal readonly FunctionSyntax Syntax;

    [DisableDump]
    internal IEnumerable<FunctionType> Functions => FindRecentCompoundView.Functions(Syntax);

    public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
    {
        FindRecentCompoundView = compoundView;
        Syntax = syntax;
    }

    CompoundView IChild<CompoundView>.Parent => FindRecentCompoundView;

    Result IConversion.Execute(Category category)
    {
        NotImplementedMethod(category);
        return null;
    }

    TypeBase IConversion.Source => this;
    IFunction IEvalImplementation.Function => this;
    IValue IEvalImplementation.Value => this;

    Result IFunction.GetResult(Category category, TypeBase argsType)
    {
        var trace = ObjectId == -49 && category.Replenished().HasClosures();
        StartMethodDump(trace, category, argsType);
        try
        {
            BreakExecution();

            var functionType = Function(argsType.AssertNotNull());

            Dump("functionType", functionType);
            BreakExecution();

            var result = functionType.ApplyResult(category);
            (category == result.CompleteCategory).Assert();

            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    bool IFunction.IsImplicit => Syntax.IsImplicit;

    IMeta IMetaImplementation.Function => null;

    IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
        => Feature.Extension.Value(DumpPrintTokenResult, this);

    Result IValue.Execute(Category category)
    {
        NotImplementedMethod(category);
        return null;
    }

    [DisableDump]
    internal override Root Root => FindRecentCompoundView.Root;

    [DisableDump]
    internal override bool IsHollow => true;

    [DisableDump]
    internal override IImplementation FunctionDeclarationForType => this;

    protected override CodeBase DumpPrintCode()
        => CodeBase.DumpPrintText(Syntax.Tag);

    new Result DumpPrintTokenResult(Category category)
        => Root.VoidType
            .GetResult(category, DumpPrintCode);

    FunctionType Function(TypeBase argsType) => FindRecentCompoundView.Function(Syntax, argsType.AssertNotNull());
}