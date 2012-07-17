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
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    sealed class ArrayAccessType : SetterTargetType
    {
        readonly Array _array;

        internal ArrayAccessType(Array array) { _array = array; }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        internal override TypeBase ValueType { get { return _array.ElementType; } }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize + _array.IndexSize; }

        internal override Result DestinationResult(Category category) { return Result(category, this); }

        internal override Result SetterResult(Category category)
        {
            return new Result
                (category
                 , getCode: () => Pair(ValueType.SmartPointer).ArgCode.ArrayAssignment(ValueType.Size, _array.IndexSize)
                 , getArgs: CodeArgs.Arg
                );
        }

        internal override Result GetterResult(Category category)
        {
            var pointer = ValueType.UniquePointer;
            var codeAndRefs =
                new Result
                    (category
                     , getType: () => pointer
                     , getCode: () => ArrayAccess
                    );

            return pointer.Result(category, codeAndRefs);
        }
        CodeBase ArrayAccess { get { return ArgCode.ArrayAccess(ValueType.Size, _array.IndexSize); } }
    }
}