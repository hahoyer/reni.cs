//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    internal abstract class StackData : DumpableObject
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

            return new BitsStackData(GetBitsConst().Resize(dataSize),OutStream);
        }

        internal StackData BitArrayBinaryOp(string opToken, Size size, StackData right)
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

        internal StackData Dereference(Size size, Size dataSize) => GetAddress().Dereference(size, dataSize);

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