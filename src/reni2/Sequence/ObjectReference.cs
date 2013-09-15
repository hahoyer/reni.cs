#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;

namespace Reni.Sequence
{
    sealed class ObjectReference : DumpableObject, IContextReference
    {
        static int _nextObjectId;

        [EnableDump]
        readonly SequenceType _objectType;

        [DisableDump]
        readonly RefAlignParam _refAlignParam;
        readonly int _order;

        internal ObjectReference(SequenceType objectType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _objectType = objectType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-1);
        }

        int IContextReference.Order { get { return _order; } }
        Size IContextReference.Size { get { return _refAlignParam.RefSize; } }
        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + _objectType.NodeDump + ")"; }
        internal Result Result(Category category) { return _objectType.ReferenceInCode(category, this); }
    }
}