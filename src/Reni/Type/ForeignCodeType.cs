using System.Reflection;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Helper;
using Reni.Runtime;

namespace Reni.Type;

sealed class ForeignCodeType : TypeBase, IImplementation, IFunction
{
    internal sealed class Entry
    {
        internal Method[]? Methods;
        readonly Dictionary<string, Entry> Data = new();

        internal Entry this[string namePart] => Data[namePart];

        internal Entry GetOrAdd(string namePart)
            => Data.TryGetValue(namePart, out var value)? value : Data[namePart] = new();
    }

    internal sealed class Method : DumpableObject, IFunction
    {
        internal readonly MethodInfo MethodInfo;
        readonly Root Root;

        TypeBase ResultType => GetLeftSideType(MethodInfo.ReturnType);

        TypeBase[] ArgsTypes
            => MethodInfo
                .GetParameters()
                .Select(GetType)
                .ToArray();

        TypeBase ArgsType
            => ArgsTypes.Aggregate(Root.VoidType, (c, n) => c.GetPair(n));


        internal Method(MethodInfo methodInfo, Root root)
        {
            MethodInfo = methodInfo;
            Root = root;
        }

        Result IFunction.GetResult(Category category, TypeBase argsType)
            => GetResult(category, argsType);


        bool IFunction.IsImplicit => false;

        Closures GetClosure()
        {
            NotImplementedMethod();
            return default!;
        }

        CodeBase GetCode()
            => ArgsType
                .ArgumentCode
                .GetForeignCall(MethodInfo, ResultType.Size);

        TypeBase GetType(ParameterInfo argument)
            => GetRightSideType(argument.ParameterType);

        TypeBase GetRightSideType(System.Type target)
        {
            if(target == typeof(string))
                return (Root.BitType * 8).TextItem.Pointer;

            NotImplementedMethod(target);
            return default!;
        }

        TypeBase GetLeftSideType(System.Type target)
        {
            if(target == typeof(string))
                return (Root.BitType * 8).TextItem.Pointer;

            NotImplementedMethod(target);
            return default!;
        }

        internal bool IsConvertibleFrom(TypeBase argsType) 
            => argsType.IsConvertible(ArgsType);

        internal Result GetResult(Category category, TypeBase argsType)
            => ResultType
                .GetResult(category, GetCode)
                .ReplaceArguments(argsType.GetConversion(category, ArgsType));
    }

    static ForeignCodeType()
    {
        var handlers = Tracer.Dumper.Configuration.Handlers;
        handlers.Add(typeof(MethodInfo), (_, o)
                =>
            {
                var methodInfo = (MethodInfo)o;
                return methodInfo.DumpMethod(true) + "=>" + methodInfo.ReturnType.PrettyName();
            }
        );
        handlers.Add(typeof(ParameterInfo), (_, o) =>
        {
            var parameter = (ParameterInfo)o;
            return parameter.ParameterType.PrettyName() + " " + parameter.Name;
        });
    }

    [DisableDump]
    internal override Root Root { get; }

    [EnableDump]
    readonly string Module;

    [EnableDump]
    readonly string[] EntryPath;

    public ForeignCodeType(string module, string entry, Root root)
    {
        Module = module;
        EntryPath = entry.Split(' ');
        Root = root;
    }

    IFunction IEvalImplementation.Function => this;

    IValue? IEvalImplementation.Value
    {
        get
        {
            NotImplementedMethod();
            return default;
        }
    }

    Result IFunction.GetResult(Category category, TypeBase argsType)
    {
        var methods = GetMethod().Where(me => me.IsConvertibleFrom(argsType)).ToArray();

        if(methods.Length == 1)
            return methods[0].GetResult(category, argsType);

        NotImplementedMethod(category, argsType, "m", methods, "argsType", argsType.StripConversionsFromPointer);
        return default!;
    }

    bool IFunction.IsImplicit => false;

    IMeta? IMetaImplementation.Function => default;

    [DisableDump]
    internal override bool IsHollow => true;

    [DisableDump]
    internal override IImplementation FunctionDeclarationForType => this;

    internal static Entry CreateEntries(string name, Root root)
    {
        var nameHead = name == "coreLib"? "Reni.Runtime.Core." : null;

        var types =
                nameHead == null
                    ? Assembly.LoadFile(name).GetExportedTypes()
                    : typeof(Data).Assembly.GetTypes().Where(isRuntimeCore)
            ;

        var result = new Entry();
        foreach(var type in types)
        {
            var rawNamespace = type.FullName ?? type.Name;
            var typeEntry = (nameHead == null? rawNamespace : rawNamespace[nameHead.Length..])
                .Split('.')
                .Aggregate(result, (current, namePart) => current.GetOrAdd(namePart));

            foreach(
                var group in
                type
                    .GetMethods()
                    .Where(isRelevantMethod)
                    .Select(m => new Method(m, root))
                    .GroupBy(m => m.MethodInfo.Name))
                typeEntry.GetOrAdd(group.Key).Methods = group.ToArray();
        }

        return result;

        bool isRuntimeCore(System.Type type) => type.FullName!.StartsWith(nameHead);

        static bool isRelevantMethod(MemberInfo info)
        {
            var methodInfo = info as MethodInfo;
            return methodInfo != null && methodInfo.IsPublic && methodInfo.IsStatic;
        }
    }

    Method[] GetMethod()
        => EntryPath
            .Aggregate(Root.ForeignLibrariesCache[Module], (current, namePart) => current[namePart])
            .Methods
            .ExpectNotNull();
}
