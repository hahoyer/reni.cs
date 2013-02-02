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
using Reni.Sequence;
using Reni.Type;

namespace Reni.TokenClasses
{
    abstract class CompareOperator : SequenceOfBitOperation
    {
        [EnableDumpExcept(true)]
        protected override bool IsCompareOperator { get { return true; } }
        protected override int ResultSize(int objSize, int argSize) { return 1; }
    }

    sealed class Equal : CompareOperator, ICompareResult
    {}

    sealed class NotEqual : CompareOperator, ICompareResult
    {}
    sealed class Less : CompareOperator, ICompareResult
    { }

    sealed class Greater: CompareOperator, ICompareResult
    { }
    sealed class LessEqual: CompareOperator, ICompareResult
    { }

    sealed class GreaterEqual: CompareOperator, ICompareResult
    { }
}