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
    internal sealed class LocalReference : FiberHead, IReferenceInCode
    {
        private readonly RefAlignParam _refAlignParam;
        private static int _nextObjectId;

        [Node]
        private readonly CodeBase _unalignedCode;

        [Node]
        [DisableDump]
        internal readonly CodeBase DestructorCode;

        public LocalReference(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            _unalignedCode = code;
            DestructorCode = destructorCode;
            StopByObjectId(-10);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal CodeBase Code { get { return _unalignedCode.Align(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }
        protected override Refs GetRefsImplementation() { return _unalignedCode.Refs + DestructorCode.Refs; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalReference(this); }

        internal CodeBase AccompayningDestructorCode(ref Size size, string holder)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(null, LocalVariableReference(RefAlignParam, holder));
        }
    }
}