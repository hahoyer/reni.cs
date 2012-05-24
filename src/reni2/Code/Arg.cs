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
using Reni.Type;

namespace Reni.Code
{
    sealed class Arg : FiberHead
    {
        static int _nextObjectId;
        readonly TypeBase _type;

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            _type = type;
            StopByObjectId(-9);
        }

        [Node]
        internal TypeBase Type { get { return _type; } }

        protected override Size GetSize() { return _type.Size; }
        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Arg(); }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }

        internal override RefAlignParam RefAlignParam
        {
            get
            {
                var reference = _type as IReference;
                return reference == null ? null : reference.RefAlignParam;
            }
        }
    }
}