//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Reference shift
    /// </summary>
    [Serializable]
    internal sealed class RefPlus : FiberItem
    {
        private readonly RefAlignParam _refAlignParam;

        [DisableDump]
        private readonly Size _right;

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        public RefPlus(RefAlignParam refAlignParam, Size right, string reason = null)
            : base(reason)
        {
            _refAlignParam = refAlignParam;
            _right = right;
            AssertValid();
            StopByObjectId(-8);
        }

        private void AssertValid()
        {
            _right.AssertAlignedSize(RefAlignParam.AlignBits);
            Tracer.Assert(!_right.IsZero);
        }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Right=" + _right; } }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.RefPlus(GetSize(), _right); }

        [DisableDump]
        internal override Size InputSize { get { return GetSize(); } }

        [DisableDump]
        internal override Size OutputSize { get { return GetSize(); } }

        private Size GetSize() { return RefAlignParam.RefSize; }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopRef(RefAlignParam, precedingElement.Offset + _right);
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrameRef(RefAlignParam, precedingElement.Offset + _right);
        }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override FiberItem[] TryToCombineBack(RefPlus precedingElement)
        {
            if(RefAlignParam.IsEqual(precedingElement.RefAlignParam))
            {
                var newRight = _right + precedingElement._right;
                if(newRight.IsZero)
                    return new FiberItem[0];
                return new[] {new RefPlus(RefAlignParam, newRight)};
            }
            return base.TryToCombineBack(precedingElement);
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return CodeBase
                .LocalVariableReference(RefAlignParam, precedingElement.Holder, precedingElement.Offset + _right);
        }
    }
}