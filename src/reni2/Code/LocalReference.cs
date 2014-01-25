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
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class LocalReference : FiberHead
    {
        static int _nextObjectId;

        [Node]
        readonly CodeBase _unalignedCode;

        [Node]
        [DisableDump]
        internal readonly CodeBase DestructorCode;

        public LocalReference(CodeBase code, CodeBase destructorCode)
            : base(_nextObjectId++)
        {
            _unalignedCode = code;
            DestructorCode = destructorCode;
            StopByObjectId(-8);
        }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        internal CodeBase Code { get { return _unalignedCode.Align(); } }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        protected override CodeArgs GetRefsImplementation() { return _unalignedCode.CodeArgs + DestructorCode.CodeArgs; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalReference(this); }

        internal CodeBase AccompayningDestructorCode(ref Size size, string holder)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(null, LocalVariableReference(holder));
        }
    }
}