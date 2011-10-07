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
    sealed class LocalVariableReference : FiberHead
    {
        static int _nextObjectId;
        readonly RefAlignParam _refAlignParam;
        [Node]
        internal readonly string Holder;
        [Node]
        internal readonly Size Offset;

        public LocalVariableReference(RefAlignParam refAlignParam, string holder, Size offset)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            Holder = holder;
            Offset = offset ?? Size.Zero;
            StopByObjectId(-1);
        }

        [DisableDump]
        [Node]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        public override string NodeDump
        {
            get
            {
                return base.NodeDump
                       + " Holder=" + Holder
                       + " Offset=" + Offset;
            }
        }

        protected override Size GetSize() { return _refAlignParam.RefSize; }
        internal override void Visit(IVisitor visitor) { visitor.LocalVariableReference(Holder, Offset); }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}