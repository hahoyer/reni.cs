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
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Dereferencing operation
    /// </summary>
    [Serializable]
    sealed class Dereference : FiberItem
    {
        readonly Size _outputSize;
        readonly Size _dataSize;
        static int _nextObjectId;

        public Dereference(Size outputSize, Size dataSize)
            : base(_nextObjectId++)
        {
            _outputSize = outputSize;
            _dataSize = dataSize;
            StopByObjectId(-6);
        }

        [DisableDump]
        internal Size DataSize { get { return _dataSize; } }

        internal override string GetNodeDump() { return base.GetNodeDump() + " DataSize=" + DataSize; }

        [DisableDump]
        internal override Size InputSize { get { return Root.DefaultRefAlignParam.RefSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _outputSize; } }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override CodeBase TryToCombineBack(TopRef precedingElement) { return new TopData(precedingElement.Offset, OutputSize, DataSize); }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            return
                new LocalVariableAccess(precedingElement.Holder, precedingElement.Offset, OutputSize, _dataSize);
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement) { return new TopFrameData(precedingElement.Offset, OutputSize, DataSize); }

        internal override void Visit(IVisitor visitor) { visitor.Dereference(OutputSize, DataSize); }
    }
}