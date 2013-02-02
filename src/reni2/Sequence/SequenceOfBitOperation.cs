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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Sequence
{
    abstract class SequenceOfBitOperation
        : Defineable<SequenceOfBitOperation>
          , ISearchPath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, BitType>
          , ISequenceOfBitBinaryOperation
    {
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> ISearchPath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, BitType>.Convert(BitType type)
        {
            if(IsCompareOperator)
                return new CompareFeature(this, type);
            return new Feature(this, type);
        }

        [DisableDump]
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }

        int ISequenceOfBitBinaryOperation.ResultSize(int objBitCount, int argBitCount) { return ResultSize(objBitCount, argBitCount); }

        protected abstract int ResultSize(int objSize, int argSize);

        [DisableDumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
    }
}