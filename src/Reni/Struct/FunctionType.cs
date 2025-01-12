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
    internal readonly TypeBase ArgumentsType;

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

    Closures ClosureValue;

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

    Closures GetterClosures => Getter.Closures;//CheckClosureValue(Getter.Closures);

    internal FunctionType
        (int index, FunctionSyntax body, CompoundView compoundView, TypeBase argumentsType)
    {
        Getter = new(this, index, body.Getter);
        Setter = body.Setter == null? null : new SetterFunction(this, index, body.Setter);
        Index = index;
        Body = body;
        CompoundView = compoundView;
        ArgumentsType = argumentsType;
        StopByObjectIds();
    }

    protected override bool IsMutable => Setter != null;

    [DisableDump]
    internal override TypeBase ValueType => Getter.ReturnType;

    [DisableDump]
    internal override bool IsHollow => GetterClosures.IsNone && ArgumentsType.IsHollow;

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
            result += ArgumentsType.DumpPrintText;
            result += ")=>";
            result += valueType?.DumpPrintText ?? "<unknown>";
            return result;
        }
    }

    protected override Result GetSetterResult(Category category) => Setter.GetCallResult(category);

    protected override Result GetGetterResult(Category category) => Getter.GetCallResult(category);
    protected override Size GetSize() => ArgumentsType.Size + GetterClosures.Size;

    internal ContextBase CreateSubContext(bool useValue)
        => new Context.Function(CompoundView.Context, ArgumentsType, useValue? ValueType : null);

    public string DumpFunction()
    {
        var result = "\n";
        result += "index=" + Index;
        result += "\n";
        result += "argsType=" + ArgumentsType.Dump();
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
                () => GetterClosures.GetCode() + ArgumentsType.ArgumentCode,
                () => GetterClosures + Closures.GetArgument()
            );
            result.CompleteCategory.Contains(category).Assert();
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    Closures CheckClosureValue(Closures result)
    {
        //return result;
        if(ObjectId == -25)
            $"{NodeDump}: {result.LogDump()}".Log();
        
        (result != null).Assert();

        if(ArgumentsType is IContextReference arguments)
            (!result!.Contains(arguments)).Assert();

        if(ClosureValue == null)
            ClosureValue = result;
        else
            (ClosureValue == result).Assert();

        return result;
    }
}