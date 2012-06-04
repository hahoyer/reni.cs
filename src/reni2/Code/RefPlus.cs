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
    ///     Reference shift
    /// </summary>
    [Serializable]
    sealed class RefPlus : FiberItem
    {
        [DisableDump]
        readonly Size _right;

        public RefPlus(Size right, string reason = null)
            : base(reason)
        {
            _right = right;
            AssertValid();
            StopByObjectId(-8);
        }

        void AssertValid()
        {
            _right.AssertAlignedSize(Root.DefaultRefAlignParam.AlignBits);
            Tracer.Assert(!_right.IsZero);
        }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Right=" + _right; } }

        internal override void Visit(IVisitor visitor) { visitor.RefPlus(_right); }

        [DisableDump]
        internal override Size InputSize { get { return Root.DefaultRefAlignParam.RefSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return Root.DefaultRefAlignParam.RefSize; } }

        internal override CodeBase TryToCombineBack(TopRef precedingElement) { return new TopRef(precedingElement.Offset + _right); }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement) { return new TopFrameRef(precedingElement.Offset + _right); }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override FiberItem[] TryToCombineBack(RefPlus precedingElement)
        {
            var newRight = _right + precedingElement._right;
            if(newRight.IsZero)
                return new FiberItem[0];
            return new FiberItem[] {new RefPlus(newRight)};
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            return CodeBase
                .LocalVariableReference(precedingElement.Holder, precedingElement.Offset + _right);
        }
    }
}