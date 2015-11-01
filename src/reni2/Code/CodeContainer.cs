using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    public sealed class CodeContainer : DumpableObject
    {
        readonly string ModuleName;
        readonly Root RootContext;
        [Node]
        readonly ValueCache<Container> MainCache;
        [Node]
        readonly FunctionCache<int, FunctionContainer> _functions;
        readonly ValueCache<string> CSharpStringCache;

        internal CodeContainer(string moduleName, Root rootContext, Syntax syntax, string description)
        {
            ModuleName = moduleName;
            RootContext = rootContext;
            MainCache = new ValueCache<Container>
                (() => rootContext.MainContainer(syntax, description));
            CSharpStringCache = new ValueCache<string>(GetCSharpStringForCache);
            _functions = new FunctionCache<int, FunctionContainer>(RootContext.FunctionContainer);
        }

        internal IEnumerable<Issue> Issues
            => Main
                .Issues
                .Union(Functions.SelectMany(f => f.Value.Issues));

        FunctionCache<int, FunctionContainer> Functions
        {
            get
            {
                for(var i = 0; i < RootContext.FunctionCount; i++)
                    _functions.IsValid(i, true);
                return _functions;
            }
        }

        Container Main => MainCache.Value;

        internal void Execute(IExecutionContext context) => Main.Data.Execute(context);

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
            for(var i = 0; i < RootContext.FunctionCount; i++)
                result += "function index=" + i + "\n" + _functions[i].Dump() + "\n";
            return result;
        }
    }
}