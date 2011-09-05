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

using System.Diagnostics;
using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;

namespace Reni.Runtime
{
    public static class DataHandler
    {
        internal static unsafe int RefBytes
        {
            get
            {
                var bytes = Root.DefaultRefAlignParam.RefSize.SaveByteCount;
                Tracer.Assert(bytes == 4);
                Tracer.Assert(sizeof(byte*) == 4);
                Tracer.Assert(sizeof(int) == 4);
                return bytes;
            }
        }

        /// <summary>
        ///     Moves the bytes.
        /// </summary>
        /// <param name = "count">The count.</param>
        /// <param name = "destination">The destination.</param>
        /// <param name = "destByte">The destination byte.</param>
        /// <param name = "source">The source.</param>
        /// created 08.10.2006 17:43
        internal static unsafe void MoveBytes(int count, byte[] destination, int destByte, Int64 source)
        {
            fixed(byte* destPtr = &destination[destByte])
                MoveBytes(count, destPtr, (byte*) &source);
        }

        /// <summary>
        ///     Moves the bytes.
        /// </summary>
        /// <param name = "count">The count.</param>
        /// <param name = "destination">The destination.</param>
        /// <param name = "source">The source.</param>
        /// created 08.10.2006 17:43
        internal static unsafe void MoveBytes(int count, byte* destination, byte* source)
        {
            if(destination < source)
                for(var i = 0; i < count; i++)
                    destination[i] = source[i];
            else if(destination > source)
                for(var i = count - 1; i >= 0; i--)
                    destination[i] = source[i];
        }

        /// <summary>
        ///     Moves the bytes with offset.
        /// </summary>
        /// <param name = "count">The count.</param>
        /// <param name = "destination">The destination.</param>
        /// <param name = "destOffset">The destination offset.</param>
        /// <param name = "source">The source.</param>
        /// <param name = "sourceOffset">The source offset.</param>
        /// created 08.10.2006 20:07
        internal static void MoveBytes(int count, byte[] destination, int destOffset, byte[] source, int sourceOffset)
        {
            for(var i = 0; i < count; i++)
                destination[i + destOffset] = source[i + sourceOffset];
        }

        /// <summary>
        ///     Dumps the print.
        /// </summary>
        /// <param name = "s">The s.</param>
        /// created 08.01.2007 18:42
        internal static void DumpPrint(string s) { BitsConst.OutStream.Add(s); }

        internal static unsafe void BitCast(this byte[] x, int bits)
        {
            fixed(byte* xx = x)
                BitCast(x.Length, xx, bits);
        }

        static unsafe void BitCast(int count, byte* x, int bitsToCast)
        {
            var isNegative = IsNegative(x[count - 1]);
            while(bitsToCast >= 8)
            {
                count--;
                x[count] = (byte) (isNegative ? -1 : 0);
                bitsToCast -= 8;
            }
            if(bitsToCast > 0)
            {
                count--;
                var @sbyte = (int) (sbyte) x[count];
                var sbyte1 = (@sbyte << bitsToCast);
                var i = (sbyte1 >> bitsToCast);
                x[count] = (byte) i;
            }
            if(bitsToCast < 0)
                throw new NotImplementedException();
        }

        static bool IsNegative(byte x) { return x >= 0x80; }

        public static void Set(byte[] dest, int destStart, params byte[] source) { source.CopyTo(dest, destStart); }

        internal static unsafe byte[] Address(this byte[] data, int dataStart)
        {
            var bytes = RefBytes;

            var result = new byte[bytes];
            fixed (byte* dataPointer = data)
            {
                var intPointer = (int) (dataPointer + dataStart);
                var bytePointer = (byte*) &intPointer;
                for(var i = 0; i < bytes; i++)
                    result[i] = bytePointer[i];
            }
            return result;
        }

        public static unsafe byte[] Dereference(this byte[] data, int dataStart, int bytes)
        {
            Tracer.Assert(data.Length >= dataStart + RefBytes);
            var result = new byte[bytes];
            fixed (byte* dataPointer = &data[dataStart])
            {
                var bytePointer = *(byte**)dataPointer;
                for (var i = 0; i < bytes; i++)
                    result[i] = bytePointer[i];
            }
            return result;
        }

        internal static unsafe void DoRefPlus(this byte[] data, int dataStart, int offset)
        {
            Tracer.Assert(data != null, "data != null");
            var result = new byte[RefBytes];
            fixed (byte* dataPointer = &data[dataStart])
            {
                var intPointer = (int*) dataPointer;
                *intPointer += offset;
            }
        }

        internal static byte[] Get(this byte[] source, int sourceStart, int bytes)
        {
            var result = new byte[bytes];
            for(var i = 0; i < bytes; i++)
                result[i] = source[sourceStart+i];
            return result;
        }

        public static unsafe void BitCast(byte[] data, int dataStart, int bytes, int bits)
        {
            fixed(byte* dataPointer = &data[dataStart])
                BitCast(bytes, dataPointer, bits);
        }

        internal static void PrintNumber(this byte[] data)
        {
            PrintText(new BigInteger(data).ToString());
        }

        internal static void PrintText(this string text) { BitsConst.OutStream.Add(text); }
        internal static void PrintText(this byte[] text) { new string(text.Select(x=>(char)x).ToArray()).PrintText(); }
        
        internal static unsafe void AssignFromPointers(this byte[] leftData, byte[] rightData, int bytes)
        {
            fixed(byte* leftPointer = leftData)
            fixed(byte* rightPointer = rightData)
                MoveBytes(bytes, *(byte**) leftPointer, *(byte**) rightPointer);
        }

        internal static bool IsLessEqual(byte[] left, byte[] right) { return !IsGreater(left, right); }
        internal static bool IsGreaterEqual(byte[] left, byte[] right) { return !IsLess(left, right); }
        internal static bool IsNotEqual(byte[] left, byte[] right) { return !IsEqual(left, right); }
        internal static bool IsLess(byte[] left, byte[] right) { return IsGreater(right, left); }

        internal static bool IsGreater(byte[] left, byte[] right)
        {
            var leftBytes = left.Length;
            var rightBytes = right.Length;
            var isLeftNegative = left[leftBytes - 1] < 0;
            var isRightNegative = right[rightBytes - 1] < 0;
            if (isLeftNegative != isRightNegative)
                return isRightNegative;

            for (var i = Math.Max(leftBytes, rightBytes) - 1; i >= 0; i--)
            {
                var leftByte = (sbyte)(isLeftNegative ? -1 : 0);
                var rightByte = (sbyte)(isRightNegative ? -1 : 0);
                if (i < leftBytes)
                    leftByte = (sbyte)left[i];
                if (i < rightBytes)
                    rightByte = (sbyte)right[i];
                if (leftByte < rightByte)
                    return false;
                if (leftByte > rightByte)
                    return true;
            }

            return false;
        }

        internal static bool IsEqual(byte[] left, byte[] right)
        {
            var leftBytes = left.Length;
            var rightBytes = right.Length;
            var d = 0;
            var i = 0;
            for (; i < leftBytes && i < rightBytes; i++)
            {
                d = (sbyte)left[i];
                if (d != (sbyte)right[i])
                    return false;
            }
            for (; i < leftBytes; i++)
            {
                if (d < 0 && (sbyte)left[i] != -1)
                    return false;
                if (d >= 0 && (sbyte)left[i] != 0)
                    return false;
            }
            for (; i < rightBytes; i++)
            {
                if (d < 0 && (sbyte)right[i] != -1)
                    return false;
                if (d >= 0 && (sbyte)right[i] != 0)
                    return false;
            }
            return true;
        }

        internal static byte[] Plus(int bytes, byte[] left, byte[] right)
        {
            var leftBytes = left.Length;
            var rightBytes = right.Length;
            var result = new byte[bytes];
            var d = 0;
            var carry = 0;

            for (var i = 0; i < bytes; i++)
            {
                if(i < leftBytes)
                    carry += ((sbyte)left[i] & 0xff);
                if(i < rightBytes)
                {
                    d = (sbyte)right[i];
                    carry += (d & 0xff);
                }
                else if(d < 0)
                    carry += 0xff;

                result[i] = (byte) (carry & 0xff);
                carry >>= 8;
            }
            return result;
        }
        
        internal static byte[] PlusSimple(byte[] left, byte[] right)
        {
            Tracer.Assert(left.Length == right.Length);
            var bytes = left.Length;
            var result = new byte[bytes];
            var carry = 0;
            for (var i = 0; i < bytes; i++)
            {
                carry += (sbyte) left[i] & 0xff;
                carry += (sbyte) right[i] & 0xff;
                result[i] = (byte) (carry & 0xff);
                carry >>= 8;
            }
            return result;
        }
        
        internal static unsafe void MinusPrefix(byte[] data)
        {
            var carry = 1;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)((sbyte)(~(sbyte)data[i]) + carry);
                carry = data[i] == 0 ? 1 : 0;
            }
        }
    }
    class Dummy : ReniObject
    {
        /// <summary>
        ///     Casts x by number of bits given.
        /// </summary>
        /// <param name = "count">The count.</param>
        /// <param name = "x">The x.</param>
        /// <param name = "bits">The bits.</param>
        /// created 03.02.2007 01:39
        [UsedImplicitly]
        public static unsafe void BitCast(int count, sbyte* x, int bits)
        {
            var isNegative = x[count - 1] < 0;
            while (bits >= 8)
            {
                count--;
                x[count] = (sbyte)(isNegative ? -1 : 0);
                bits -= 8;
            }
            if (bits > 0)
            {
                count--;
                x[count] = (sbyte)((sbyte)(x[count] << bits) >> bits);
            }
            if (bits < 0)
            {
                NotImplementedFunction(count, x[0], bits);
                throw new NotImplementedException();
            }
        }

        static unsafe void BoolToSBytes(int countResult, sbyte* dataResult, bool result)
        {
            var value = (sbyte)(result ? -1 : 0);
            for (var i = 0; i < countResult; i++)
                dataResult[i] = value;
        }
    }
}