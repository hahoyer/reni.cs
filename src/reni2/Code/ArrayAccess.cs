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
using Reni.Context;

namespace Reni.Code
{
    sealed class ArrayAccess : FiberItem
    {
        internal readonly Size ElementSize;
        internal readonly Size IndexSize;
        readonly string _callingMethodName;
        public ArrayAccess(Size elementSize, Size indexSize, string callingMethodName)
        {
            ElementSize = elementSize;
            IndexSize = indexSize;
            _callingMethodName = callingMethodName;
        }
        internal override Size InputSize { get { return Root.DefaultRefAlignParam.RefSize + IndexSize; } }
        internal override Size OutputSize { get { return Root.DefaultRefAlignParam.RefSize; } }
        internal override void Visit(IVisitor visitor) { visitor.ArrayAccess(ElementSize,IndexSize); }
        protected override Size GetAdditionalTemporarySize() { return Root.DefaultRefAlignParam.RefSize * 2; }
    }
}