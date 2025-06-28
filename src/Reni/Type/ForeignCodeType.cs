using System.Reflection;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Helper;
using Reni.Runtime;

namespace Reni.Type;

sealed class ForeignCodeType : TypeBase, IImplementation, IFunction
{
    static ForeignCodeType()
    {
        var handlers = Tracer.Dumper.Configuration.Handlers;
        handlers.Add(typeof(MethodBase), (_, o) => ((MethodBase)o).DumpMethod(true));
        handlers.Add(typeof(ParameterInfo), (_, o) =>
        {
            var parameter = (ParameterInfo)o;
            return parameter.ParameterType.PrettyName() + " " + parameter.Name;
        });
    }

    internal sealed class TransferResult { }
    internal sealed class Entry
    {
        internal Method[]? Methods;
        readonly Dictionary<string, Entry> Data = new();

        internal Entry this[string namePart] => Data[namePart];

        internal Entry GetOrAdd(string namePart)
            => Data.TryGetValue(namePart, out var value)? value : Data[namePart] = new();
    }

    internal sealed class Method :DumpableObject
    {
        internal readonly MethodInfo MethodInfo;
        readonly Root Root;


        internal Method(MethodInfo methodInfo, Root root)
        {
            MethodInfo = methodInfo;
            Root = root;
        }

        internal TransferResult Transfer(TypeBase argsType)
        {
            var targetArgsType = GetType(MethodInfo.GetParameters());
            var targetResultType = GetLeftSideType(MethodInfo.ReturnType);

            NotImplementedMethod(argsType);
            return default!;
        }

        TypeBase GetType(ParameterInfo[] arguments)
        {
            if(arguments.Length == 1)
                return GetRightSideType(arguments[0].ParameterType);

            NotImplementedMethod(arguments);
            return default!;
        }

        TypeBase GetRightSideType(System.Type target)
        {
            if(target== typeof(string))
                return (Root.BitType * 8).TextItem.Pointer;

            NotImplementedMethod(target);
            return default!;
        }

        TypeBase GetLeftSideType(System.Type target)
        {
            if(target== typeof(string))
                return (Root.BitType * 8).TextItem.Pointer;

            NotImplementedMethod(target);
            return default!;
        }
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
        var m = GetMethod().Select(m=>m.Transfer(argsType)).ToArray();

        NotImplementedMethod(category, argsType, "m", m);
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
                    .Select(m => new Method(m,root))
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
