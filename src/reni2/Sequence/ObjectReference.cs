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
using Reni.Basics;
using Reni.Code;

namespace Reni.Sequence
{
    sealed class ObjectReference : ReniObject, IReferenceInCode
    {
        static int _nextObjectId;

        [EnableDump]
        readonly SequenceType _objectType;

        [DisableDump]
        readonly RefAlignParam _refAlignParam;

        internal ObjectReference(SequenceType objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-1);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return _refAlignParam; } }
        internal override string DumpShort() { return base.DumpShort() + "(" + _objectType.DumpShort() + ")"; }
        internal Result Result(Category category) { return _objectType.ReferenceInCode(category, this); }
        string IDumpShortProvider.DumpShort() { return DumpShort(); }
    }
}
