using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using hw.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Code
{
    public sealed class CodeContainer : DumpableObject
    {
        readonly string ModuleName;
        readonly Root Root;
        [Node]
        readonly ValueCache<Container> MainCache;
        [Node]
        readonly FunctionCache<int, FunctionContainer> _functions;
        readonly ValueCache<string> CSharpStringCache;

        internal CodeContainer
            (string moduleName, Root root, Syntax syntax, string description)
        {
            ModuleName = moduleName;
            Root = root;
            MainCache = new ValueCache<Container>
                (() => root.MainContainer(syntax, description));
            CSharpStringCache = new ValueCache<string>(GetCSharpStringForCache);
            _functions = new FunctionCache<int, FunctionContainer>(Root.FunctionContainer);
        }

        internal IEnumerable<Issue> Issues
            => Main
                .Issues
                .plus(Functions.SelectMany(f => f.Value.Issues));

        FunctionCache<int, FunctionContainer> Functions
        {
            get
            {
                for(var i = 0; i < Root.FunctionCount; i++)
                    _functions.IsValid(i, true);
                return _functions;
            }
        }

        internal Container Main => MainCache.Value;

        internal void Execute(IExecutionContext context, ITraceCollector traceCollector)
            => Main.Data.Execute(context, traceCollector);

        string GetCSharpStringForCache()
            => ModuleName.CreateCSharpString(Main, Functions);

        [DisableDump]
        internal string CSharpString => CSharpStringCache.Value;

        internal CodeBase Function(FunctionId functionId)
        {
            var item = _functions[functionId.Index];
            var container = functionId.IsGetter ? item.Getter : item.Setter;
            return container.Data;
        }

        public override string DumpData()
        {
            var result = "main\n" + Main.Dump() + "\n";
            for(var i = 0; i < Root.FunctionCount; i++)
                result += "function index=" + i + "\n" + _functions[i].Dump() + "\n";
            return result;
        }
    }
}