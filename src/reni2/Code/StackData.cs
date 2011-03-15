using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Type;

namespace Reni.Code
{
    internal abstract class StackData : ReniObject
    {
        internal virtual StackData Push(StackData stackData)
        {
            NotImplementedMethod(stackData);
            return null;
        }

        internal virtual StackData PushOnto(NonListStackData formerStack)
        {
            NotImplementedMethod(formerStack);
            return null;
        }

        protected virtual StackData GetTop(Size size)
        {
            NotImplementedMethod(size);
            return null;
        }

        protected virtual StackData Pull(Size size)
        {
            NotImplementedMethod(size);
            return null;
        }

        internal abstract Size Size { get; }

        internal virtual StackData PushOnto(NonListStackData[] formerStack)
        {
            NotImplementedMethod(formerStack);
            return null;
        }

        internal void DumpPrintOperation() { GetBitsConst().PrintNumber(); }

        internal StackData DoGetTop(Size size)
        {
            if(size == Size)
                return this;
            return GetTop(size);
        }

        internal StackData DoPull(Size size)
        {
            if(size.IsZero)
                return this;
            if(size == Size)
                return new EmptyStackData();
            return Pull(size);
        }

        internal StackData BitCast(Size dataSize)
        {
            if(Size == dataSize)
                return this;

            return new BitsStackData(GetBitsConst().Resize(dataSize).Resize(Size));
        }

        internal StackData BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, StackData right)
        {
            var leftData = GetBitsConst();
            var rightData = right.GetBitsConst();
            var resultData = leftData.BitArrayBinaryOp(opToken.DataFunctionName, size, rightData);
            return new BitsStackData(resultData);
        }

        internal StackData RefPlus(Size offset) { return GetAddress().RefPlus(offset); }

        internal StackData Dereference(Size size, Size dataSize) { return GetAddress().Dereference(size, dataSize); }

        protected virtual StackDataAddress GetAddress()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual BitsConst GetBitsConst()
        {
            NotImplementedMethod();
            return null;
        }
    }
}