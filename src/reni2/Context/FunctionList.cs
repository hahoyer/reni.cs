#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    ///     List of functions
    /// </summary>
    [Serializable]
    sealed class FunctionList : ReniObject
    {
        [Node]
        readonly DictionaryEx<FunctionSyntax, DictionaryEx<Structure, DictionaryEx<TypeBase, int>>> _dictionary;

        [Node]
        readonly List<Struct.FunctionType> _list = new List<Struct.FunctionType>();

        public FunctionList()
        {
            _dictionary = new DictionaryEx<FunctionSyntax, DictionaryEx<Structure, DictionaryEx<TypeBase, int>>>
                (syntax => new DictionaryEx<Structure, DictionaryEx<TypeBase, int>>
                               (structure => new DictionaryEx<TypeBase, int>
                                                 (-1, args => CreateFunctionInstance(args, syntax, structure))));
        }

        internal Struct.FunctionType this[int i] { get { return _list[i]; } }
        internal int Count { get { return _list.Count; } }
        internal CodeBasePair[] Code { get { return _list.Select(t => t.BodyCode).ToArray(); } }

        internal Struct.FunctionType Find(FunctionSyntax syntax, Structure structure, TypeBase argsType)
        {
            var index = _dictionary.Find(syntax).Find(structure).Find(argsType);
            return _list[index];
        }

        internal List<FunctionContainer> Compile() { return _list.Select(t => t.Serialize()).ToList(); }

        int CreateFunctionInstance(TypeBase args, FunctionSyntax syntax, Structure structure)
        {
            var index = _list.Count;
            var f = new Struct.FunctionType(index, syntax, structure, args);
            _list.Add(f);
            return index;
        }
    }
}