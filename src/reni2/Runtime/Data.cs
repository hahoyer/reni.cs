using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace Reni.Runtime
{
    /// <summary>
    /// Data class that contains data for a stack layer
    /// </summary>
    [UsedImplicitly]
    public static class Data 
    {
        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="destByte">The destination byte.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 17:43
        internal static unsafe void MoveBytes(int count, byte[] destination, int destByte, Int64 source)
        {
            fixed(byte* destPtr = &destination[destByte])
                MoveBytes(count, destPtr, (byte*) &source);
        }

        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 17:43
        private static unsafe void MoveBytes(int count, byte* destination, byte* source)
        {
            if(destination < source)
                for(var i = 0; i < count; i++)
                    destination[i] = source[i];
            else if(destination > source)
                for(var i = count - 1; i >= 0; i--)
                    destination[i] = source[i];
        }

        /// <summary>
        /// Moves the bytes with offset.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="destOffset">The destination offset.</param>
        /// <param name="source">The source.</param>
        /// <param name="sourceOffset">The source offset.</param>
        /// created 08.10.2006 20:07
        internal static void MoveBytes(int count, byte[] destination, int destOffset, byte[] source, int sourceOffset)
        {
            for(var i = 0; i < count; i++)
                destination[i + destOffset] = source[i + sourceOffset];
        }

        /// <summary>
        /// Dumps the print.
        /// </summary>
        /// <param name="s">The s.</param>
        /// created 08.01.2007 18:42
        internal static void DumpPrint(string s)
        {
            BitsConst.OutStream.Add(s);
        }

        internal static unsafe void BitCast(byte[] x, int bits)
        {
            fixed(byte* xx = x)
                BitCast(x.Length, xx, bits);
        }

        private static unsafe void BitCast(int count, byte* x, int bitsToCast)
        {
            var isNegative = x[count - 1].IsNegative();
            while(bitsToCast >= 8)
            {
                count--;
                x[count] = (byte) (isNegative ? -1 : 0);
                bitsToCast -= 8;
            }
            if(bitsToCast > 0)
            {
                count--;
                var @sbyte = (int)(sbyte)x[count];
                var sbyte1 = (@sbyte << bitsToCast);
                var i = (sbyte1 >> bitsToCast);
                x[count] = (byte) i;
            }
            if(bitsToCast < 0)
            {
                throw new NotImplementedException();
            }
        }

        static bool IsNegative(this byte x) { return x >= 0x80; }

        [UsedImplicitly]
        public static void Set(byte[] dest, int destStart, params byte[] source)
        {
            source.CopyTo(dest,destStart);
        }

        [UsedImplicitly]
        public static unsafe void SetFromPointer(byte[] dest, int destStart, int bytes, byte[] source, int sourceStart)
        {
            Tracer.Assert(bytes == 4);
            Tracer.Assert(sizeof(byte*) == 4);
            Tracer.Assert(sizeof(int) == 4);

            fixed (byte* sourcePointer = source)
            {
                var intPointer = (int)(sourcePointer + sourceStart);
                var bytePointer = (byte*)&intPointer;
                for (var i = 0; i < bytes; i++)
                    dest[destStart + i] = bytePointer[i];
            }
        }

        [UsedImplicitly]
        public static byte[] Get(byte[] source, int sourceStart, int bytes)
        {
            var result = new byte[bytes];
            for (var i = 0; i < bytes; i++)
                result[i] = source[sourceStart];
            return result;
        }

        [UsedImplicitly]
        public static unsafe void Dereference(byte[] data, int dataStart, int pointerSize, int bytes)
        {
            Tracer.Assert(pointerSize == 4);
            Tracer.Assert(sizeof(byte*) == 4);

            var result = new byte[bytes];
            fixed (byte* sourcePointer = &data[dataStart])
            {
                var ipp = (byte**)sourcePointer;
                for (var i = 0; i < bytes; i++)
                    result[i] = (*ipp)[i];
            }
            for (var i = 0; i < bytes; i++)
                data[dataStart + pointerSize - bytes] = result[i];
        }

        [UsedImplicitly]
        public static unsafe void BitCast(byte[] data, int dataStart, int bytes, int bits)
        {
            fixed (byte* dataPointer = &data[dataStart])
                BitCast(bytes, dataPointer, bits);
        }

        [UsedImplicitly]
        public static void DumpPrint(byte[] data, int dataStart, int bytes)
        {
            var piece = new byte[bytes];
            for (var i = 0; i < bytes; i++)
                piece[i] = data[dataStart + i];
            BitsConst.OutStream.Add(new BigInteger(piece).ToString());
        }

    }
}