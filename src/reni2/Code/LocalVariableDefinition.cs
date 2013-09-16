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
using hw.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    sealed class LocalVariableDefinition : FiberItem
    {
        [Node]
        readonly string _holderName;
        readonly Size _valueSize;

        public LocalVariableDefinition(string holderName, Size valueSize)
        {
            _holderName = holderName;
            _valueSize = valueSize;
            StopByObjectId(-4);
        }

        [DisableDump]
        internal override Size InputSize { get { return _valueSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return Size.Zero; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " Holder=" + _holderName + " ValueSize=" + _valueSize; }
        internal override void Visit(IVisitor visitor) { visitor.LocalVariableDefinition(_holderName, _valueSize); }
    }
}