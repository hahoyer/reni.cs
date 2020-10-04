using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    public sealed class CodeContainer : DumpableObject
    {
        [Node]
        readonly FunctionCache<int, FunctionContainer> _functions;

        readonly ValueCache<string> CSharpStringCache;

        [Node]
        readonly ValueCache<Container> MainCache;

        readonly string ModuleName;
        readonly Root Root;

        internal CodeContainer(ValueSyntax syntax, Root root, string moduleName, string description)
        {
            ModuleName = moduleName;
            Root = root;
            MainCache = new ValueCache<Container>(() => root.MainContainer(syntax, description));
            CSharpStringCache = new ValueCache<string>(GetCSharpStringForCache);
            _functions = new FunctionCache<int, FunctionContainer>(Root.FunctionContainer);
        }

        internal IEnumerable<Issue> Issues
            => Main
                .Issues
                .plus(Functions.SelectMany(f => f.Value.Issues));

        internal Container Main => MainCache.Value;

        [DisableDump]
        internal string CSharpString => CSharpStringCache.Value;

        FunctionCache<int, FunctionContainer> Functions
        {
            get
            {
                for(var i = 0; i < Root.FunctionCount; i++)
                    _functions.IsValid(i, true);
                return _functions;
            }
        }

        public override string DumpData()
        {
            var result = "main\n" + Main.Dump() + "\n";
            for(var i = 0; i < Root.FunctionCount; i++)
                result += "function index=" + i + "\n" + _functions[i].Dump() + "\n";
            return result;
        }

        internal void Execute(IExecutionContext context, ITraceCollector traceCollector)
            => Main.Data.Execute(context, traceCollector);

        internal CodeBase Function(FunctionId functionId)
        {
            var item = _functions[functionId.Index];
            var container = functionId.IsGetter? item.Getter : item.Setter;
            return container.Data;
        }

        string GetCSharpStringForCache()
            => ModuleName.CreateCSharpString(Main, Functions);
    }
}