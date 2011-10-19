//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    ///     List of functions
    /// </summary>
    [Serializable]
    internal sealed class FunctionList : ReniObject
    {
        [Node]
        private readonly DictionaryEx<CompileSyntax, DictionaryEx<Structure, DictionaryEx<TypeBase, int>>> _dictionary;

        [Node]
        private readonly List<FunctionInstance> _list = new List<FunctionInstance>();

        public FunctionList() {
            _dictionary = new DictionaryEx<CompileSyntax, DictionaryEx<Structure, DictionaryEx<TypeBase, int>>>
            (body => new DictionaryEx<Structure, DictionaryEx<TypeBase, int>>
                (structure => new DictionaryEx<TypeBase, int>
                    (-1, args => CreateFunctionInstance(args, body, structure))));
        }

        internal FunctionInstance this[int i] { get { return _list[i]; } }
        internal int Count { get { return _list.Count; } }
        internal CodeBase[] Code { get { return _list.Select(t => t.BodyCode).ToArray(); } }

        internal FunctionInstance Find(CompileSyntax body, Structure structure, TypeBase argsType)
        {
            var index = _dictionary.Find(body).Find(structure).Find(argsType);
            return _list[index];
        }

        internal List<Code.Container> Compile() { return _list.Select(t => t.Serialize(false)).ToList(); }

        private int CreateFunctionInstance(TypeBase args, CompileSyntax body, Structure structure)
        {
            var index = _list.Count;
            var f = new FunctionInstance(index, body, structure, args);
            _list.Add(f);
            return index;
        }
    }
}