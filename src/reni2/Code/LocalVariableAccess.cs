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

namespace Reni.Code
{
    sealed class LocalVariableAccess : FiberHead
    {
        static int _nextObjectId;
        [DisableDump]
        [Node]
        internal readonly string Holder;
        [DisableDump]
        [Node]
        internal readonly Size Offset;
        readonly Size _size;
        [DisableDump]
        [Node]
        internal readonly Size DataSize;

        public LocalVariableAccess(string holder, Size offset, Size size, Size dataSize)
            : base(_nextObjectId++)
        {
            Holder = holder;
            Offset = offset;
            _size = size;
            DataSize = dataSize;
            StopByObjectId(-10);
        }

        internal override string GetNodeDump()
        {
            return base.GetNodeDump()
                   + " Holder=" + Holder
                   + " Offset=" + Offset
                   + " DataSize=" + DataSize
                ;
        }

        protected override Size GetSize() { return _size; }
        internal override void Visit(IVisitor visitor) { visitor.LocalVariableAccess(Holder, Offset, Size, DataSize); }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}