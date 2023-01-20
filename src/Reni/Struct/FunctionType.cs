using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

sealed class FunctionType : SetterTargetType
{
    [Node]
    internal readonly TypeBase ArgsType;

    [DisableDump]
    internal readonly FunctionSyntax Body;

    [NotNull]
    [Node]
    [EnableDump]
    internal readonly GetterFunction Getter;

    [EnableDump]
    internal readonly int Index;

    [Node]
    [EnableDump]
    internal readonly SetterFunction Setter;

    [Node]
    [EnableDump]
    readonly CompoundView CompoundView;

    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            if(IsMutable)
                yield return ReassignToken.TokenId;

            if(Body.IsImplicit)
                foreach(var option in ValueType.DeclarationOptions)
                    yield return option;
        }
    }


    [Node]
    [DisableDump]
    internal Closures Closures => GetClosures();

    [DisableDump]
    internal FunctionContainer Container
    {
        get
        {
            var getter = Getter.Container;
            var setter = Setter?.Container;
            return new(getter, setter);
        }
    }

    [DisableDump]
    internal IEnumerable<IFormalCodeItem> CodeItems
    {
        get
        {
            var getter = Getter.CodeItems;
            var setter = Setter?.CodeItems;
            return setter == null? getter : getter.Concat(setter);
        }
    }

    internal FunctionType
        (int index, FunctionSyntax body, CompoundView compoundView, TypeBase argsType)
    {
        Getter = new(this, index, body.Getter);
        Setter = body.Setter == null? null : new SetterFunction(this, index, body.Setter);
        Index = index;
        Body = body;
        CompoundView = compoundView;
        ArgsType = argsType;
        StopByObjectIds();
    }

    protected override bool IsMutable => Setter != null;

    [DisableDump]
    internal override TypeBase ValueType => Getter.ReturnType;

    [DisableDump]
    internal override bool IsHollow => Closures.IsNone && ArgsType.IsHollow;

    [DisableDump]
    internal override CompoundView FindRecentCompoundView => CompoundView;

    [DisableDump]
    internal override bool HasQuickSize => false;

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    internal override Issue[] Issues
        => Setter == null
            ? Getter.Issues
            : Getter.Issues.Concat(Setter.Issues).ToArray();

    internal override string DumpPrintText
    {
        get
        {
            var valueType = ValueType;
            var result = "@(";
            result += ArgsType.DumpPrintText;
            result += ")=>";
            result += valueType?.DumpPrintText ?? "<unknown>";
            return result;
        }
    }

    protected override Result SetterResult(Category category) => Setter.GetCallResult(category);

    protected override Result GetterResult(Category category) => Getter.GetCallResult(category);
    protected override Size GetSize() => ArgsType.Size + Closures.Size;

    internal ContextBase CreateSubContext(bool useValue)
        => new Context.Function(CompoundView.Context, ArgsType, useValue? ValueType : null);

    public string DumpFunction()
    {
        var result = "\n";
        result += "index=" + Index;
        result += "\n";
        result += "argsType=" + ArgsType.Dump();
        result += "\n";
        result += "context=" + CompoundView.Dump();
        result += "\n";
        result += "Getter=" + Getter.DumpFunction();
        result += "\n";
        if(Setter != null)
        {
            result += "Setter=" + Setter.DumpFunction();
            result += "\n";
        }

        result += "type=" + ValueType.Dump();
        result += "\n";
        return result;
    }

    // remark: watch out when caching anything here. 
    // This may hinder the recursive call detection, located at result cache of context. 
    public Result ApplyResult(Category category)
    {
        var trace = Index.In();
        StartMethodDump(trace, category);
        try
        {
            BreakExecution();
            var result = GetResult
            (
                category,
                () => Closures.ToCode() + ArgsType.ArgCode,
                () => Closures + Closures.Arg()
            );
            (category == result.CompleteCategory).Assert();
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    Closures GetClosures()
    {
        var result = Getter.Closures;
        (result != null).Assert();
        if(Setter != null)
            result += Setter.Closures;
        var argsExt = ArgsType as IContextReference;
        if(argsExt != null)
            (!result.Contains(argsExt)).Assert();
        return result;
    }
}