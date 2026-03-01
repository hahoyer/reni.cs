using System.Numerics;
using Reni.Context;

namespace Reni.Runtime;

static class DataHandler
{
    internal sealed class RuntimeException : Exception
    {
        public RuntimeException(Exception exception)
            : base("Runtime exception during Dereference.", exception) { }
    }

    internal static int RefBytes
        => Root.DefaultRefAlignParam.RefSize.SaveByteCount;

    /// <summary>
    ///     Moves the bytes.
    /// </summary>
    /// <param name="count"> The count. </param>
    /// <param name="destination"> The destination. </param>
    /// <param name="destByte"> The destination byte. </param>
    /// <param name="source"> The source. </param>
    /// created 08.10.2006 17:43
    internal static unsafe void MoveBytes
        (int count, byte[] destination, int destByte, long source)
    {
        fixed(byte* destPtr = &destination[destByte])
            MoveBytes(count, destPtr, (byte*)&source);
    }

    /// <summary>
    ///     Moves the bytes.
    /// </summary>
    /// <param name="count"> The count. </param>
    /// <param name="destination"> The destination. </param>
    /// <param name="source"> The source. </param>
    /// created 08.10.2006 17:43
    static unsafe void MoveBytes(int count, byte* destination, byte* source)
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
    /// <param name="count"> The count. </param>
    /// <param name="destination"> The destination. </param>
    /// <param name="destOffset"> The destination offset. </param>
    /// <param name="source"> The source. </param>
    /// <param name="sourceOffset"> The source offset. </param>
    /// created 08.10.2006 20:07
    internal static void MoveBytes
        (int count, byte[] destination, int destOffset, byte[] source, int sourceOffset)
    {
        for(var i = 0; i < count; i++)
            destination[i + destOffset] = source[i + sourceOffset];
    }

    static unsafe void BitCast(int count, byte* x, int bitsToCast)
    {
        var isNegative = IsNegative(x[count - 1]);
        while(bitsToCast >= 8)
        {
            count--;
            x[count] = (byte)(isNegative? -1 : 0);
            bitsToCast -= 8;
        }

        if(bitsToCast > 0)
        {
            count--;
            var @sbyte = (int)(sbyte)x[count];
            var sbyte1 = @sbyte << bitsToCast;
            var i = sbyte1 >> bitsToCast;
            x[count] = (byte)i;
        }

        if(bitsToCast < 0)
            throw new NotImplementedException();
    }

    static bool IsNegative(byte x) => x >= 0x80;

    public static void Set(byte[] dest, int destStart, params byte[] source) => source.CopyTo(dest, destStart);

    [UsedImplicitly]
    public static unsafe void BitCast(byte[] data, int dataStart, int bytes, int bits)
    {
        fixed(byte* dataPointer = &data[dataStart])
            BitCast(bytes, dataPointer, bits);
    }

    internal static bool IsLessEqual(byte[] left, byte[] right) => !IsGreater(left, right);
    internal static bool IsGreaterEqual(byte[] left, byte[] right) => !IsLess(left, right);
    internal static bool IsNotEqual(byte[] left, byte[] right) => !IsEqual(left, right);
    internal static bool IsLess(byte[] left, byte[] right) => IsGreater(right, left);

    internal static bool IsGreater(byte[] left, byte[] right)
    {
        var leftBytes = left.Length;
        var rightBytes = right.Length;
        var isLeftNegative = left[leftBytes - 1] > 127;
        var isRightNegative = right[rightBytes - 1] > 127;
        if(isLeftNegative != isRightNegative)
            return isRightNegative;

        for(var i = Math.Max(leftBytes, rightBytes) - 1; i >= 0; i--)
        {
            var leftByte = (sbyte)(isLeftNegative? -1 : 0);
            var rightByte = (sbyte)(isRightNegative? -1 : 0);
            if(i < leftBytes)
                leftByte = (sbyte)left[i];
            if(i < rightBytes)
                rightByte = (sbyte)right[i];
            if(leftByte < rightByte)
                return false;
            if(leftByte > rightByte)
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
        for(; i < leftBytes && i < rightBytes; i++)
        {
            d = (sbyte)left[i];
            if(d != (sbyte)right[i])
                return false;
        }

        for(; i < leftBytes; i++)
        {
            if(d < 0 && (sbyte)left[i] != -1)
                return false;
            if(d >= 0 && (sbyte)left[i] != 0)
                return false;
        }

        for(; i < rightBytes; i++)
        {
            if(d < 0 && (sbyte)right[i] != -1)
                return false;
            if(d >= 0 && (sbyte)right[i] != 0)
                return false;
        }

        return true;
    }

    extension(byte[] data)
    {
        internal unsafe byte[] Pointer(int dataStart)
        {
            var bytes = RefBytes;

            var result = new byte[bytes];
            fixed(byte* dataPointer = data)
            {
                (sizeof(long) == sizeof(byte*)).Assert(()
                    => $"sizeof(long) = {sizeof(long)}, sizeof(byte*) = {sizeof(byte*)}");
                var intPointer = (long)(dataPointer + dataStart);
                var bytePointer = (byte*)&intPointer;
                for(var i = 0; i < bytes; i++)
                    result[i] = bytePointer[i];
            }

            return result;
        }

        public unsafe byte[] Dereference(int dataStart, int bytes)
        {
            try
            {
                (data.Length >= dataStart + RefBytes).Assert();
                var result = new byte[bytes];
                fixed(byte* dataPointer = &data[dataStart])
                {
                    var bytePointer = *(byte**)dataPointer;
                    for(var i = 0; i < bytes; i++)
                        result[i] = bytePointer[i];
                }

                return result;
            }
            catch(AccessViolationException exception)
            {
                throw new RuntimeException(exception);
            }
        }

        internal unsafe void DoRefPlus(int dataStart, int offset)
        {
            fixed(byte* dataPointer = &data[dataStart])
            {
                var intPointer = (int*)dataPointer;
                *intPointer += offset;
            }
        }

        internal byte[] Get(int sourceStart, int bytes)
        {
            var result = new byte[bytes];
            for(var i = 0; i < bytes; i++)
                result[i] = data[sourceStart + i];
            return result;
        }

        internal void PrintNumber() => PrintText(new BigInteger(data).ToString());
        internal void PrintText() => new string(data.Select(x => (char)x).ToArray()).PrintText();

        internal unsafe void AssignFromPointers
            (byte[] rightData, int bytes)
        {
            fixed(byte* leftPointer = data)
            fixed(byte* rightPointer = rightData)
                MoveBytes(bytes, *(byte**)leftPointer, *(byte**)rightPointer);
        }

        internal byte[] Plus(byte[] right, int bytes)
        {
            var leftBytes = data.Length;
            var rightBytes = right.Length;
            var result = new byte[bytes];
            var d = 0;
            var carry = 0;

            for(var i = 0; i < bytes; i++)
            {
                if(i < leftBytes)
                    carry += (sbyte)data[i] & 0xff;
                if(i < rightBytes)
                {
                    d = (sbyte)right[i];
                    carry += d & 0xff;
                }
                else if(d < 0)
                    carry += 0xff;

                result[i] = (byte)(carry & 0xff);
                carry >>= 8;
            }

            return result;
        }

        [UsedImplicitly]
        internal byte[] PlusSimple(byte[] right)
        {
            (data.Length == right.Length).Assert();
            var bytes = data.Length;
            var result = new byte[bytes];
            var carry = 0;
            for(var i = 0; i < bytes; i++)
            {
                carry += (sbyte)data[i] & 0xff;
                carry += (sbyte)right[i] & 0xff;
                result[i] = (byte)(carry & 0xff);
                carry >>= 8;
            }

            return result;
        }

        internal void MinusPrefix()
        {
            var carry = 1;
            for(var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)((sbyte)~(sbyte)data[i] + carry);
                carry = data[i] == 0? 1 : 0;
            }
        }

        internal byte[] Times(byte[] right, int bytes)
            => (new BigInteger(data) * new BigInteger(right))
                .ToByteArray()
                .ByteAlign(bytes);

        byte[] ByteAlign(int bytes)
        {
            if(data.Length == bytes)
                return data;
            var result = new byte[bytes];
            var i = 0;
            for(; i < bytes && i < data.Length; i++)
                result[i] = data[i];
            if(i < bytes)
            {
                var sign = (byte)(data[i - 1] >= 128? 127 : 0);
                for(; i < bytes; i++)
                    result[i] = sign;
            }

            return result;
        }

        internal byte[] Times(int right, int bytes)
            => (new BigInteger(data) * new BigInteger(right))
                .ToByteArray()
                .ByteAlign(bytes);
    }

    extension(string text)
    {
        internal void PrintText() => Data.OutStream?.AddData(text);
    }
}