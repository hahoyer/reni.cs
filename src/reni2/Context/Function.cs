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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    sealed class Function : Child, IFunctionContext
    {
        [Node]
        internal readonly TypeBase ArgsType;
        [Node]
        internal readonly TypeBase ValueType;
        internal Function(ContextBase parent, TypeBase argsType, TypeBase valueType = null)
            : base(parent)
        {
            ArgsType = argsType;
            ValueType = valueType;
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        Size IReferenceInCode.RefSize { get { return RefAlignParam.RefSize; } }

        internal override IFunctionContext ObtainRecentFunctionContext() { return this; }

        Result IFunctionContext.CreateArgReferenceResult(Category category)
        {
            return ArgsType
                .ContextAccessResult(category.Typed, this, ArgsType.Size * -1)
                & category;
        }

        Result IFunctionContext.CreateValueReferenceResult(Category category)
        {
            if(ValueType == null)
                throw new ValueCannotBeUsedHereException();
            return ValueType
                .ContextAccessResult(category.Typed, this, (ArgsType.Size + ValueType.Size) * -1)
                & category;
        }
    }

    sealed class ValueCannotBeUsedHereException : Exception
    {}

    interface IFunctionContext : IReferenceInCode
    {
        Result CreateArgReferenceResult(Category category);
        Result CreateValueReferenceResult(Category category);
    }
}