using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reni.Context;

namespace Reni.Runtime
{
    /// <summary>
    /// Data class that contains data for a stack layer
    /// </summary>
    [UsedImplicitly]
    public sealed class Data : ReniObject
    {
        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="destByte">The destination byte.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 17:43
        public static unsafe void MoveBytes(int count, byte[] destination, int destByte, Int64 source)
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
        public static unsafe void MoveBytes(int count, byte* destination, byte* source)
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
        public static void MoveBytes(int count, byte[] destination, int destOffset, byte[] source, int sourceOffset)
        {
            for(var i = 0; i < count; i++)
                destination[i + destOffset] = source[i + sourceOffset];
        }

        private static unsafe void MoveBytes(int count, byte* destination, byte[] source)
        {
            for (var i = 0; i < count; i++)
                destination[i] = source[i];
        }

        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 20:07
        public static void MoveBytes(int count, byte[] destination, byte[] source)
        {
            for(var i = 0; i < count; i++)
                destination[i] = source[i];
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int64 x)
        {
            BitsConst.Convert(x).PrintNumber();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int32 x)
        {
            BitsConst.Convert(x).PrintNumber();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int16 x)
        {
            BitsConst.Convert(x).PrintNumber();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        [UsedImplicitly]
        public static void DumpPrint(byte x)
        {
            BitsConst.Convert(x).PrintNumber();
        }

        /// <summary>
        /// Dumps the print.
        /// </summary>
        /// <param name="s">The s.</param>
        /// created 08.01.2007 18:42
        [UsedImplicitly]
        public static void DumpPrint(string s)
        {
            BitsConst.OutStream.Add(s);
        }

        /// <summary>
        /// Bitses the array.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// created 02.02.2007 01:01
        [UsedImplicitly]
        public static unsafe void BitsArray(int count, sbyte* destination, params byte[] source)
        {
            fixed(byte* s = &source[0])
                MoveBytes(count, (byte*) destination, s);
        }

        /// <summary>
        /// Tops the data.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// created 02.02.2007 01:09
        public static unsafe void MoveBytes(int count, sbyte* destination, sbyte* source)
        {
            MoveBytes(count, (byte*) destination, (byte*) source);
        }

        [UsedImplicitly]
        public static unsafe void DumpPrint(byte[] source)
        {
            var count = source.Length;
            var data = ToInt64(count, source);
            BitsConst.Convert(data).PrintNumber();
        }

        public static unsafe Int64 ToInt64(int count, byte[] source)
        {
            Int64 data = source[count - 1] < 0 ? -1 : 0;
            MoveBytes(count, (byte*) &data, source);
            return data;
        }

        internal static unsafe void BitCast(byte[] x, int bits)
        {
            fixed(byte* xx = x)
                BitCast(x.Length, xx, bits);
        }

        internal static unsafe void BitCast(int count, byte* x, int bits)
        {
            var isNegative = x[count - 1] >= 0x80;
            while(bits >= 8)
            {
                count--;
                x[count] = (byte) (isNegative ? -1 : 0);
                bits -= 8;
            }
            if(bits > 0)
            {
                count--;
                var @sbyte = (int)x[count];
                var sbyte1 = (@sbyte << bits);
                var i = (sbyte1 >> bits);
                x[count] = (byte) i;
            }
            if(bits < 0)
            {
                NotImplementedFunction(count, x[0], bits);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Changes the sign of data.
        /// </summary>
        /// <param name="countResult">The count result.</param>
        /// <param name="data">The data.</param>
        /// <param name="countData">The count data.</param>
        /// created 03.02.2007 01:32
        internal static unsafe void Minus(int countResult, byte* data, int countData)
        {
            var carry = 1;
            for(var i = 0; i < countData; i++)
            {
                data[i] = (byte) ((byte) (~data[i]) + carry);
                carry = data[i] == 0 ? 1 : 0;
            }

            if(countResult != countData)
            {
                NotImplementedFunction(countResult, data[0], countData);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Minuses the specified count result.
        /// </summary>
        /// <param name="countResult">The count result.</param>
        /// <param name="dataResult">The data result.</param>
        /// <param name="count1st">The count1st.</param>
        /// <param name="data1st">The data1st.</param>
        /// <param name="count2nd">The count2nd.</param>
        /// <param name="data2nd">The data2nd.</param>
        /// created 03.02.2007 20:48
        [UsedImplicitly]
        public static unsafe void Minus(int countResult, byte* dataResult, int count1st, byte* data1st, int count2nd, byte* data2nd)
        {
            fixed(byte* m = new byte[count2nd])
            {
                MoveBytes(count2nd, m, data2nd);
                Minus(count2nd, m, count2nd);
                Plus(countResult, dataResult, count1st, data1st, count2nd, m);
            }
        }

        public static unsafe void Plus(byte[] dataResult, int count1st, byte[] data)
        {
            fixed(byte* pnewData = dataResult)
            fixed(byte* poldData = data)
                Plus(dataResult.Length, pnewData, count1st, poldData, data.Length - count1st, poldData + count1st);
        }
        
        [UsedImplicitly]
        public static unsafe void Plus(int countResult, byte* dataResult, int count1st, byte* data1st, int count2nd, byte* data2nd)
        {
            var d = 0;
            var carry = 0;
            for(var i = 0; i < countResult; i++)
            {
                if(i < count1st)
                    carry += (data1st[i] & 0xff);
                if(i < count2nd)
                {
                    d = data2nd[i];
                    carry += (d & 0xff);
                }
                else if(d < 0)
                    carry += 0xff;

                dataResult[i] = (byte) (carry & 0xff);
                carry >>= 8;
            }
        }

        public static unsafe bool Equal(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            var d = 0;
            var i = 0;
            for(; i < count1st && i < count2nd; i++)
            {
                d = data1st[i];
                if(d != data2nd[i])
                    return false;
            }
            for(; i < count1st; i++)
            {
                if(d < 0 && data1st[i] != -1)
                    return false;
                if(d >= 0 && data1st[i] != 0)
                    return false;
            }
            for(; i < count2nd; i++)
            {
                if(d < 0 && data2nd[i] != -1)
                    return false;
                if(d >= 0 && data2nd[i] != 0)
                    return false;
            }
            return true;
        }

        public static unsafe bool Greater(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            var isNegative1st = data1st[count1st - 1] < 0;
            var isNegative2nd = data2nd[count2nd - 1] < 0;
            if(isNegative1st != isNegative2nd)
                return isNegative2nd;

            for(var i = Math.Max(count1st, count2nd) - 1; i >= 0; i--)
            {
                var x1st = (sbyte) (isNegative1st ? -1 : 0);
                var x2nd = (sbyte) (isNegative2nd ? -1 : 0);
                if(i < count1st)
                    x1st = data1st[i];
                if(i < count2nd)
                    x2nd = data2nd[i];
                if(x1st < x2nd)
                    return false;
                if(x1st > x2nd)
                    return true;
            }

            return false;
        }

        [UsedImplicitly]
        public static unsafe void Greater(int countResult, sbyte* dataResult, int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            BoolToSBytes(countResult, dataResult, Greater(count1st, data1st, count2nd, data2nd));
        }

        [UsedImplicitly]
        public static unsafe void Less(int countResult, sbyte* dataResult, int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            BoolToSBytes(countResult, dataResult, Less(count1st, data1st, count2nd, data2nd));
        }

        [UsedImplicitly]
        public static unsafe void Equal(int countResult, sbyte* dataResult, int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            BoolToSBytes(countResult, dataResult, Equal(count1st, data1st, count2nd, data2nd));
        }

        private static unsafe void BoolToSBytes(int countResult, sbyte* dataResult, bool result)
        {
            var value = (sbyte) (result ? -1 : 0);
            for(var i = 0; i < countResult; i++)
                dataResult[i] = value;
        }

        public static unsafe bool Less(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            return Greater(count2nd, data2nd, count1st, data1st);
        }
    }
}