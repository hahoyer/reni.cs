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
        , ITemplateProvider
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

    readonly TypeBase TemplateArguments;

    [DisableDump]
    internal IEnumerable<FunctionType> Functions => FindRecentCompoundView.Functions(Syntax);

    [DisableDump]
    internal TypeBase Template => this.CachedValue(GetTemplate);

    public FunctionBodyType(CompoundView compoundView, FunctionSyntax syntax)
    {
        FindRecentCompoundView = compoundView;
        Syntax = syntax;
        TemplateArguments = new TemplateArguments(this);
        StopByObjectIds(47);
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

    Result IFunction.GetResult(Category category, TypeBase argumentsType) => GetResult(category, argumentsType);

    bool IFunction.IsImplicit => Syntax.IsImplicit;

    IMeta IMetaImplementation.Function => null;

    IImplementation ISymbolProvider<DumpPrintToken>.GetFeature(DumpPrintToken tokenClass)
        => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    Root ITemplateProvider.Root => Root;

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

    [DisableDump]
    protected override CodeBase DumpPrintCode => CodeBase.GetDumpPrintText(Syntax.Tag);

    TypeBase GetTemplate()
    {
        var argumentsType = Syntax.IsImplicit? Root.VoidType : TemplateArguments;
        return GetResult(Category.Type, argumentsType).Type;
    }

    Result GetResult(Category category, TypeBase argumentsType)
    {
        var trace = ObjectId == -49 && category.Replenished().HasClosures();
        StartMethodDump(trace, category, argumentsType);
        try
        {
            BreakExecution();

            var functionType = Function(argumentsType.AssertNotNull());

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

    new Result GetDumpPrintTokenResult(Category category)
        => Root.VoidType
            .GetResult(category, () => DumpPrintCode);

    FunctionType Function(TypeBase argsType) => FindRecentCompoundView.Function(Syntax, argsType.AssertNotNull());
}

sealed class TemplateArguments : TypeBase
{
    readonly ITemplateProvider Parent;
    public TemplateArguments(ITemplateProvider parent) => Parent = parent;

    internal override Root Root => Parent.Root;
}

interface ITemplateProvider
{
    Root Root { get; }
}