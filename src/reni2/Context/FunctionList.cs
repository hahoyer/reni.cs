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
using hw.Debug;
using hw.Helper;
using hw.Forms;
using Reni.Code;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class FunctionList : DumpableObject
    {
        [Node]
        readonly FunctionCache<FunctionSyntax, FunctionCache<Structure, FunctionCache<TypeBase, int>>> _dictionary;

        [Node]
        readonly List<FunctionType> _list = new List<FunctionType>();

        public FunctionList()
        {
            _dictionary = new FunctionCache<FunctionSyntax, FunctionCache<Structure, FunctionCache<TypeBase, int>>>
                (syntax => new FunctionCache<Structure, FunctionCache<TypeBase, int>>
                               (structure => new FunctionCache<TypeBase, int>
                                                 (-1, args => CreateFunctionInstance(args, syntax, structure))));
        }

        internal FunctionType this[int i] { get { return _list[i]; } }
        internal int Count { get { return _list.Count; } }

        internal FunctionType Find(FunctionSyntax syntax, Structure structure, TypeBase argsType)
        {
            var index = _dictionary[syntax][structure][argsType];
            return _list[index];
        }

        internal FunctionContainer Container(int index) { return _list[index].Container; }

        int CreateFunctionInstance(TypeBase args, FunctionSyntax syntax, Structure structure)
        {
            var index = _list.Count;
            var f = new FunctionType(index, syntax, structure, args);
            _list.Add(f);
            return index;
        }
    }
}