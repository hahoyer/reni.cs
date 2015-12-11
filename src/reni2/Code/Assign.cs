#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    sealed class Assign : FiberItem
    {
        [DisableDump]
        readonly RefAlignParam _refAlignParam;

        protected override string GetNodeDump() => base.GetNodeDump() + " TargetSize=" + _targetSize + " RefSize=" + _refAlignParam.RefSize;
        internal override void Visit(IVisitor visitor) => visitor.Assign(_targetSize);

        [DisableDump]
        readonly Size _targetSize;

        public Assign(RefAlignParam refAlignParam, Size targetSize)
        {
            _refAlignParam = refAlignParam;
            _targetSize = targetSize;
        }

        [DisableDump]
        internal override Size InputSize => _refAlignParam.RefSize * 2;

        [DisableDump]
        internal override Size OutputSize => Size.Zero;
    }
}