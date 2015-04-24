using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    sealed class CodeContainer : DumpableObject
    {
        readonly Root _rootContext;
        [Node]
        readonly ValueCache<Container> _mainCache;
        [Node]
        readonly FunctionCache<int, FunctionContainer> _functions;

        public CodeContainer(Root rootContext, Syntax syntax, string description)
        {
            _rootContext = rootContext;
            _mainCache = new ValueCache<Container>
                (() => rootContext.MainContainer(syntax, description));
            _functions = new FunctionCache<int, FunctionContainer>(_rootContext.FunctionContainer);
        }

        internal IEnumerable<Issue> Issues
        {
            get
            {
                return Main
                    .Issues
                    .Union(Functions.SelectMany(f => f.Value.Issues));
            }
        }

        FunctionCache<int, FunctionContainer> Functions
        {
            get
            {
                for(var i = 0; i < _rootContext.FunctionCount; i++)
                    _functions.IsValid(i, true);
                return _functions;
            }
        }

        Container Main => _mainCache.Value;

        internal void Execute(IExecutionContext context) => Main.Data.Execute(context);

        internal string CreateCSharpString(string className) => Generator
            .CreateCSharpString(Main, Functions, true, className);

        internal Assembly CreateCSharpAssembly(string className, bool generatorFilePosn)
            => Generator
                .CreateCSharpAssembly(Main, Functions, false, className, generatorFilePosn);

        public CodeBase Function(FunctionId functionId)
        {
            var item = _functions[functionId.Index];
            var container = functionId.IsGetter ? item.Getter : item.Setter;
            return container.Data;
        }

        public override string DumpData()
        {
            var result = "main\n" + Main.Dump() + "\n";
            for(var i = 0; i < _rootContext.FunctionCount; i++)
                result += "function index=" + i + "\n" + _functions[i].Dump() + "\n";
            return result;
        }
    }
}