#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.ReniParser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.Code
{
    sealed class CodeContainer : ReniObject
    {
        readonly Root _rootContext;
        [Node]
        readonly SimpleCache<Container> _mainCache;
        [Node]
        readonly DictionaryEx<int, FunctionContainer> _functions;

        public CodeContainer(Root rootContext, ParsedSyntax syntax, string description)
        {
            _rootContext = rootContext;
            _mainCache = new SimpleCache<Container>(() => rootContext.MainContainer(syntax, description));
            _functions = new DictionaryEx<int, FunctionContainer>(_rootContext.FunctionContainer);
        }

        internal IEnumerable<IssueBase> Issues
        {
            get
            {
                return Main
                    .Issues
                    .Union(Functions.SelectMany(f => f.Value.Issues));
            }
        }

        DictionaryEx<int, FunctionContainer> Functions
        {
            get
            {
                for (var i = 0; i < _rootContext.FunctionCount; i++)
                    _functions.Ensure(i);
                return _functions;
            }
        }
        Container Main { get { return _mainCache.Value; } }

        internal void Execute(IExecutionContext context) { Main.Data.Execute(context); }

        internal string CreateCSharpString(string className)
        {
            return Generator
                .CreateCSharpString(Main, Functions, true, className);
        }

        internal Assembly CreateCSharpAssembly(string className, bool generatorFilePosn)
        {
            return Generator
                .CreateCSharpAssembly(Main, Functions, false, className, generatorFilePosn);
        }

        public CodeBase Function(FunctionId functionId)
        {
            var item = _functions[functionId.Index];
            var container = functionId.IsGetter ? item.Getter : item.Setter;
            return container.Data;
        }

        public override string DumpData()
        {
            var result = "main\n" + Main.Dump() + "\n";
            for (var i = 0; i < _rootContext.FunctionCount; i++)
                result += "function index=" + i + "\n" + _functions[i].Dump() + "\n";
            return result;
        }
    }
}