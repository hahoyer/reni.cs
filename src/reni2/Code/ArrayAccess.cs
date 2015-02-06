using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    abstract class ArrayAccess : FiberItem
    {
        internal readonly Size ElementSize;
        internal readonly Size IndexSize;
        readonly string _callingMethodName;
        protected static readonly BitArrayBinaryOp AddressPlus = new BitArrayBinaryOp("Plus", Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize);
        static readonly BitArrayBinaryOp AddressMultiply = new BitArrayBinaryOp("Star", Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize, Root.DefaultRefAlignParam.RefSize);
        protected ArrayAccess(Size elementSize, Size indexSize, string callingMethodName)
        {
            ElementSize = elementSize;
            IndexSize = indexSize;
            _callingMethodName = callingMethodName;
        }
        internal override void Visit(IVisitor visitor) { NotImplementedMethod(visitor); }

        protected CodeBase IndexCalculation(CodeBase index)
        {
            var elementSize = BitsConst
                .Convert(ElementSize.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits))
                .Resize(Root.DefaultRefAlignParam.RefSize);


            var castedIndex = index.BitCast(Root.DefaultRefAlignParam.RefSize);
            if(elementSize.ToInt64() == 1)
                return castedIndex;
            return (castedIndex + CodeBase.BitsConst(elementSize))
                .Add(AddressMultiply);
        }

        protected CodeBase AddressCalculation(CodeBase[] list)
        {
            var targetArray = list[0];
            if(targetArray.Size != Root.DefaultRefAlignParam.RefSize)
                return null;

            var index = list[1];
            if(index.Size != IndexSize)
                return null;

            return (targetArray + IndexCalculation(index))
                .Add(AddressPlus);
        }
    }

    sealed class ArrayGetter : ArrayAccess
    {
        public ArrayGetter(Size elementSize, Size indexSize, string callingMethodName)
            : base(elementSize, indexSize, callingMethodName)
        { }

        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize + IndexSize;
        internal override Size OutputSize => Root.DefaultRefAlignParam.RefSize;

        internal override CodeBase TryToCombineBack(List precedingElement)
        {
            var list = precedingElement.Data;
            if (list.Length != 2)
                return null;

            return AddressCalculation(list);
        }
    }

    sealed class ArraySetter : ArrayAccess
    {
        public ArraySetter(Size elementSize, Size indexSize, string callingMethodName)
            : base(elementSize, indexSize, callingMethodName) { }

        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize * 2 + IndexSize;
        internal override Size OutputSize => Size.Zero;

        internal override CodeBase TryToCombineBack(List precedingElement)
        {
            var list = precedingElement.Data;
            if(list.Length != 3)
                return null;

            var value = list[2];
            if(value.Size != Root.DefaultRefAlignParam.RefSize)
                return null;

            return (AddressCalculation(list) + value)
                .Assignment(ElementSize);
        }
    }
}