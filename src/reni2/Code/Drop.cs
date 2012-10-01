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

namespace Reni.Code
{
    sealed class Drop : FiberItem
    {
        readonly Size _beforeSize;
        readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        internal override void Visit(IVisitor visitor) { visitor.Drop(_beforeSize, _afterSize); }

        [DisableDump]
        internal override Size InputSize { get { return _beforeSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _afterSize; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize; }
    }
}