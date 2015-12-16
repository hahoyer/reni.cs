using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    abstract class StackData : DumpableObject
    {
        internal readonly IOutStream OutStream;

        protected StackData(IOutStream outStream) { OutStream = outStream; }

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

        internal virtual StackData PushOnto(ListStack formerStack)
        {
            NotImplementedMethod(formerStack);
            return null;
        }

        internal void PrintNumber() => GetBitsConst().PrintNumber(OutStream);
        internal void PrintText(Size itemSize) => GetBitsConst().PrintText(itemSize, OutStream);

        internal StackData DoGetTop(Size size)
        {
            if(size == Size)
                return this;
            if(size.IsZero)
                return new EmptyStackData(OutStream);
            return GetTop(size);
        }

        internal StackData DoPull(Size size)
        {
            if(size.IsZero)
                return this;
            if(size == Size)
                return new EmptyStackData(OutStream);
            return Pull(size);
        }

        internal StackData BitCast(Size dataSize)
        {
            if(Size == dataSize)
                return this;

            return new BitsStackData(GetBitsConst().Resize(dataSize), OutStream);
        }

        internal virtual StackData BitArrayBinaryOp(string opToken, Size size, StackData right)
        {
            var leftData = GetBitsConst();
            var rightData = right.GetBitsConst();
            var resultData = leftData.BitArrayBinaryOp(opToken, size, rightData);
            return new BitsStackData(resultData, OutStream);
        }

        internal StackData BitArrayPrefixOp(string operation, Size size)
        {
            var argData = GetBitsConst();
            var resultData = argData.BitArrayPrefixOp(operation, size);
            return new BitsStackData(resultData, OutStream);
        }

        internal StackData RefPlus(Size offset) => GetAddress().RefPlus(offset);

        internal StackData Dereference(Size size, Size dataSize)
            => GetAddress().Dereference(size, dataSize);

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

        internal void Assign(Size size, StackData right) => GetAddress().Assign(size, right);
    }
}