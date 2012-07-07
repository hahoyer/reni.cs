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
using Reni.Basics;

namespace Reni.Type
{
    sealed class Pair : TypeBase
    {
        [EnableDump]
        readonly TypeBase _first;
        [EnableDump]
        readonly TypeBase _second;

        internal Pair(TypeBase first, TypeBase second)
        {
            _first = first;
            _second = second;
        }

        internal override bool IsDataLess { get { return _first.IsDataLess && _second.IsDataLess; } }
        protected override Size GetSize() { return _first.Size + _second.Size; }

        [DisableDump]
        internal override string DumpPrintText
        {
            get
            {
                var result = "";
                var types = ToList;
                foreach(var t in types)
                {
                    result += "\n";
                    result += t;
                }
                return "(" + result.Indent() + "\n)";
            }
        }

        [DisableDump]
        internal override TypeBase[] ToList
        {
            get
            {
                var result = new List<TypeBase>(_first.ToList) {_second};
                return result.ToArray();
            }
        }

        internal override Result Destructor(Category category)
        {
            var firstHandler = _first.Destructor(category);
            var secondHandler = _second.Destructor(category);
            if(firstHandler.IsEmpty)
                return secondHandler;
            if(secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal override string DumpShort() { return "pair." + ObjectId; }
    }
}