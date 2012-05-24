// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Dereferencing operation
    /// </summary>
    [Serializable]
    sealed class Dereference : FiberItem
    {
        readonly RefAlignParam _refAlignParam;
        readonly Size _outputSize;
        readonly Size _dataSize;
        static int _nextObjectId;

        public Dereference(RefAlignParam refAlignParam, Size outputSize, Size dataSize)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            _outputSize = outputSize;
            _dataSize = dataSize;
            StopByObjectId(-4);
        }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        internal Size DataSize { get { return _dataSize; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " DataSize=" + DataSize; } }

        [DisableDump]
        internal override Size InputSize { get { return RefAlignParam.RefSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _outputSize; } }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new LocalVariableAccess(RefAlignParam, precedingElement.Holder, precedingElement.Offset, OutputSize, _dataSize);
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
        {
            return null;
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrameData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override void Visit(IVisitor visitor) { visitor.Dereference(OutputSize, DataSize); }
    }
}