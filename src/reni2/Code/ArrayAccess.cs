#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class ArrayAccess : FiberItem
    {
        internal readonly Size ElementSize;
        internal readonly Size IndexSize;
        readonly string _callingMethodName;
        static readonly BitArrayBinaryOp _plus = new BitArrayBinaryOp("Plus", Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize);
        static readonly BitArrayBinaryOp _multiply = new BitArrayBinaryOp("Star", Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize);
        public ArrayAccess(Size elementSize, Size indexSize, string callingMethodName)
        {
            ElementSize = elementSize;
            IndexSize = indexSize;
            _callingMethodName = callingMethodName;
        }
        internal override Size InputSize { get { return Root.DefaultRefAlignParam.RefSize + IndexSize; } }
        internal override Size OutputSize { get { return Root.DefaultRefAlignParam.RefSize; } }
        internal override void Visit(IVisitor visitor) { NotImplementedMethod(visitor); }
        protected override Size GetAdditionalTemporarySize() { return Root.DefaultRefAlignParam.RefSize * 2; }

        internal override CodeBase TryToCombineBack(List precedingElement)
        {
            var list = precedingElement.Data;
            if (list.Length != 2)
                return null;
            
            var targetArray = list[0];
            if (targetArray.Size != Root.DefaultRefAlignParam.RefSize)
                return null;

            var index = list[1];
            if (index.Size != IndexSize)
                return null;

            return targetArray.Sequence(IndexCalculation(index)).Add(_plus);
        }
        CodeBase IndexCalculation(CodeBase index)
        {
            var elementSize = BitsConst
                .Convert(ElementSize.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits))
                .Resize(Root.DefaultRefAlignParam.RefSize);
            
            return index
                .BitCast(Root.DefaultRefAlignParam.RefSize)
                .Sequence(CodeBase.BitsConst(elementSize))
                .Add(_multiply);
        }
    }
    sealed class ArrayAssignment : FiberItem
    {
        internal readonly Size ElementSize;
        internal readonly Size IndexSize;
        readonly string _callingMethodName;
        public ArrayAssignment(Size elementSize, Size indexSize, string callingMethodName)
        {
            ElementSize = elementSize;
            IndexSize = indexSize;
            _callingMethodName = callingMethodName;
        }
        internal override Size InputSize { get { return Root.DefaultRefAlignParam.RefSize * 2 + IndexSize; } }
        internal override Size OutputSize { get { return Size.Zero; } }
        internal override void Visit(IVisitor visitor) { NotImplementedMethod(visitor); }
        protected override Size GetAdditionalTemporarySize() { return Root.DefaultRefAlignParam.RefSize * 2; }
    }
}