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

namespace Reni.Code
{
    [Serializable]
    sealed class BitArray : FiberHead
    {
        readonly Size _size;

        [Node]
        [DisableDump]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            //Tracer.Assert(size.IsPositive);
            _size = size;
            Data = data;
            StopByObjectId(-21);
        }

        public BitArray()
            : this(Size.Zero, Basics.BitsConst.None()) { }

        protected override Size GetSize() { return _size; }

        protected override IEnumerable<CodeBase> AsList()
        {
            if(IsDataLess)
                return new CodeBase[0];
            return new[] {this};
        }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.BitArray(this); }

        [DisableDump]
        internal override bool IsEmpty { get { return IsDataLess; } }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override void Visit(IVisitor visitor) { visitor.BitsArray(Size, Data); }

        protected override string GetNodeDump() { return base.GetNodeDump() + " Data=" + Data; }

        internal new static BitArray Void { get { return new BitArray(Size.Create(0), Basics.BitsConst.None()); } }
    }
}