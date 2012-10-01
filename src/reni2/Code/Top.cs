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

using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    [Serializable]
    abstract class Top : FiberHead
    {
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        readonly Size _size;
        readonly Size _dataSize;

        protected Top(Size offset, Size size, Size dataSize)
        {
            Offset = offset;
            _size = size;
            _dataSize = dataSize;
            StopByObjectId(-945);
        }

        protected override Size GetSize() { return _size; }

        [DisableDump]
        internal override bool IsRelativeReference { get { return true; } }

        [Node]
        [DisableDump]
        protected Size DataSize { get { return _dataSize; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " Offset=" + Offset + " DataSize=" + _dataSize; }
    }
}