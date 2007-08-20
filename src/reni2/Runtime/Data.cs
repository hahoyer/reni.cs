using System;

namespace Reni.Runtime
{
    /// <summary>
    /// Data class that contains data for a stack layer
    /// </summary>
    public class Data : ReniObject
    {
        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="destByte">The dest byte.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 17:43
        public static unsafe void MoveBytes(int count, byte[] dest, int destByte, Int64 source)
        {
            fixed (byte* destPtr = &dest[destByte])
                MoveBytes(count, destPtr, (byte*)&source);
        }

        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 17:43
        public static unsafe void MoveBytes(int count, byte* dest, byte* source)
        {
            for (int i = 0; i < count; i++)
                dest[i] = source[i];
        }

        /// <summary>
        /// Moves the bytes with offset.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="destOffset">The dest offset.</param>
        /// <param name="source">The source.</param>
        /// <param name="sourceOffset">The source offset.</param>
        /// created 08.10.2006 20:07
        public static void MoveBytes(int count, byte[] dest, int destOffset, byte[] source, int sourceOffset)
        {
            for (int i = 0; i < count; i++)
                dest[i + destOffset] = source[i + sourceOffset];
        }

        /// <summary>
        /// Moves the bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="source">The source.</param>
        /// created 08.10.2006 20:07
        public static void MoveBytes(int count, byte[] dest, byte[] source)
        {
            for (int i = 0; i < count; i++)
                dest[i] = source[i];
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int64 x)
        {
            BitsConst.Convert(x).PrintNum();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int32 x)
        {
            BitsConst.Convert(x).PrintNum();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(Int16 x)
        {
            BitsConst.Convert(x).PrintNum();
        }

        /// <summary>
        /// Bits the array op_printnum.
        /// </summary>
        /// <param name="x">The x.</param>
        /// created 11.10.2006 01:12
        public static void DumpPrint(byte x)
        {
            BitsConst.Convert(x).PrintNum();
        }

        /// <summary>
        /// Dumps the print.
        /// </summary>
        /// <param name="s">The s.</param>
        /// created 08.01.2007 18:42
        public static void DumpPrint(string s)
        {
            BitsConst.OutStream.Add(s);
        }

        /// <summary>
        /// Bitses the array.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="source">The source.</param>
        /// created 02.02.2007 01:01
        unsafe public static void BitsArray(int count, sbyte* dest, params byte[] source)
        {
            fixed(byte* s = &source[0]) 
                MoveBytes(count,(byte*)dest,s);
        }

        /// <summary>
        /// Tops the data.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="source">The source.</param>
        /// created 02.02.2007 01:09
        unsafe public static void MoveBytes(int count, sbyte* dest, sbyte* source)
        {
            MoveBytes(count, (byte*)dest, (byte*)source);
        }

        /// <summary>
        /// Dumps the print.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="source">The source.</param>
        /// created 02.02.2007 01:10
        unsafe public static void DumpPrint(int count, sbyte* source)
        {
            Int64 data = ToInt64(count, source);
            BitsConst.Convert(data).PrintNum();
        }

        private static unsafe Int64 ToInt64(int count, sbyte* source)
        {
            Int64 data = source[count - 1] < 0 ? -1 : 0;
            MoveBytes(count, (byte*)&data, (byte*)source);
            return data;
        }

        /// <summary>
        /// Casts x by number of bits given. 
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="x">The x.</param>
        /// <param name="bits">The bits.</param>
        /// created 03.02.2007 01:39
        public static unsafe void BitCast(int count, sbyte* x, int bits)
        {
            bool isNegative = x[count - 1] < 0;
            while(bits >= 8)
            {
                count--;
                x[count] = (sbyte) (isNegative ? -1 : 0);
                bits -= 8;
            }
            if(bits > 0)
            {
                count--;
                x[count] = (sbyte)((sbyte)(x[count] << bits) >> bits);
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
        /// <param name="count">The count.</param>
        /// <param name="data">The data.</param>
        /// created 03.02.2007 01:32
        public static unsafe void Minus(int count, sbyte* data)
        {
            int carry = 1;
            for (int i = 0; i < count; i++)
            {
                data[i] = (sbyte)((sbyte)(~data[i]) + carry);
                carry = data[i] == 0 ? 1 : 0;
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
        public static unsafe void Minus(int countResult, sbyte* dataResult, int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            fixed(sbyte*m = new sbyte[count2nd])
            {
                MoveBytes(count2nd,m,data2nd);
                Minus(count2nd,m);
                Plus(countResult,dataResult,count1st,data1st,count2nd,m);
            }
        }
        /// <summary>
        /// Pluses the specified count result.
        /// </summary>
        /// <param name="countResult">The count result.</param>
        /// <param name="dataResult">The data result.</param>
        /// <param name="count1st">The count1st.</param>
        /// <param name="data1st">The data1st.</param>
        /// <param name="count2nd">The count2nd.</param>
        /// <param name="data2nd">The data2nd.</param>
        /// created 03.02.2007 20:48
        public static unsafe void Plus(int countResult, sbyte* dataResult, int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            int d = 0;
            int carry = 0;
            for (int i = 0; i < countResult; i++)
            {
                if (i < count1st)
                    carry += (data1st[i] & 0xff);
                if (i < count2nd)
                {
                    d = data2nd[i];
                    carry += (d & 0xff);
                }
                else if (d < 0)
                    carry += 0xff;

                dataResult[i] = (sbyte)(carry & 0xff);
                carry >>= 8;
            }
        }
        public static unsafe bool  Equal(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            int d = 0;
            int i = 0;
            for (; i < count1st && i < count2nd; i++)
            {
                d = data1st[i];
                if(d != data2nd[i])
                    return false;
            }
            for (; i < count1st; i++)
            {
                if (d < 0 && data1st[i] != -1) return false;
                if (d >= 0 && data1st[i] != 0) return false;
            }
            for (; i < count2nd; i++)
            {
                if (d < 0 && data2nd[i] != -1) return false;
                if (d >= 0 && data2nd[i] != 0) return false;
            }
            return true;
        }

        public static unsafe bool Greater(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            bool isNegative1st = data1st[count1st - 1] < 0;
            bool isNegative2nd = data2nd[count2nd - 1] < 0;
            if(isNegative1st != isNegative2nd)
                return isNegative2nd;

            for(int i = Math.Max(count1st, count2nd) - 1;i >= 0; i--)
            {
                sbyte x1st = (sbyte) (isNegative1st ? -1 : 0);
                sbyte x2nd = (sbyte) (isNegative2nd ? -1 : 0);
                if (i < count1st) x1st = data1st[i];
                if (i < count2nd) x2nd = data2nd[i];
                if (x1st < x2nd) return false;
                if (x1st > x2nd) return true;
            }

            return false;
        }

        public static unsafe bool Less(int count1st, sbyte* data1st, int count2nd, sbyte* data2nd)
        {
            return Greater(count2nd, data2nd, count1st, data1st);
        }
    }
}
                              