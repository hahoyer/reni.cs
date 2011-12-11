// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni
{
    abstract class ConversionFunction: ReniObject
    {
        readonly TypeBase _parent;
        static int _nextObjectId;
        protected ConversionFunction(TypeBase parent)
            : base(_nextObjectId++) { _parent = parent; }
        internal abstract Result Result(Category category);
        [DisableDump]
        internal TypeBase ArgType { get { return _parent; } }
        [DisableDump]
        internal TypeBase ResultType { get { return Result(Category.Type).Type; } }

        public override string NodeDump
        {
            get
            {
                return base.NodeDump
                       + "["
                       + (ArgType == null ? "<null>" : ArgType.NodeDump)
                       + "=>"
                       + ResultType.NodeDump
                       + "]";
            }
        }
    }
}