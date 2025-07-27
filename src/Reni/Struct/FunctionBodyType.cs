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
    [DisableDump]
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
        StopByObjectIds();
    }

    CompoundView IChild<CompoundView>.Parent => FindRecentCompoundView;

    Result IConversion.Execute(Category category)
    {
        NotImplementedMethod(category);
        return null!;
    }

    TypeBase IConversion.Source => this;
    IFunction IEvalImplementation.Function => this;
    IValue IEvalImplementation.Value => this;

    Result IFunction.GetResult(Category category, TypeBase argumentsType) => GetResult(category, argumentsType);

    bool IFunction.IsImplicit => Syntax.IsImplicit;

    IMeta? IMetaImplementation.Function => null;

    IImplementation ISymbolProvider<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    Root ITemplateProvider.Root => Root;

    Result IValue.Execute(Category category)
    {
        NotImplementedMethod(category);
        return null!;
    }

    [DisableDump]
    internal override Root Root => FindRecentCompoundView.Root;

    [DisableDump]
    internal override bool IsHollow => true;

    internal override IImplementation GetFunctionDeclarationForType() => this;

    [DisableDump]
    protected override CodeBase DumpPrintCode => Syntax.FunctionKindDump.GetDumpPrintTextCode();

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
            (result.CompleteCategory .Contains(category)).Assert();

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