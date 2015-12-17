using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using JetBrains.Annotations;
using Reni.Runtime;

namespace Reni.Basics
{
    [DumpToString]
    sealed class BitsConst : DumpableObject
    {
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal const int SegmentAlignBits = 3;
        static int _nextObjectId;

        readonly byte[] _data;
        readonly ValueCache<BigInteger> _dataCache;

        BitsConst(Size size)
            : base(_nextObjectId++)
        {
            Size = size;
            _data = CreateDataArray();
            _dataCache = new ValueCache<BigInteger>(() => new BigInteger(_data));
            StopByObjectIds(-7);
        }

        BitsConst(long value, Size size)
            : this(new BitsConst(value), size) {}

        BitsConst(long value)
            : this(Size.AutoSize(value))
        {
            DataHandler.MoveBytes(DataSize(Size), _data, 0, value);
        }

        BitsConst(BitsConst value, Size size)
            : this(size)
        {
            MoveData(_data, Size, value._data, value.Size);
        }

        BitsConst(int bytes, byte[] value, int start)
            : this(Size.Create(bytes * 8))
        {
            DataHandler.MoveBytes(DataSize(Size), _data, 0, value, start);
        }

        static Size SegmentBits => Size.Create(1 << SegmentAlignBits);
        static int SegmentValues => 1 << SegmentBits.ToInt();
        static int MaxSegmentValue => SegmentValues - 1;
        public static int BitSize(System.Type t) => Marshal.SizeOf(t) << SegmentAlignBits;
        public bool IsEmpty => Size.IsZero;
        public Size Size { get; }

        public int ByteCount => DataSize(Size);
        public bool IsZero { get { return _data.All(t => t == 0); } }

        bool IsNegative
        {
            get
            {
                if(Size.IsZero)
                    return false;
                return GetBit(Size - 1) != 0;
            }
        }

        public byte[] ToByteArray() => (byte[]) _data.Clone();

        static Size SlagBits(Size size) => SegmentBits * DataSize(size) - size;

        byte[] CreateDataArray() => new byte[DataSize(Size)];

        static void MoveData(byte[] data, Size size, byte[] source, Size sourceSize)
        {
            var i = 0;
            var n = size.Min(sourceSize).ToInt() >> SegmentAlignBits;
            for(; i < n; i++)
                data[i] = source[i];

            var sizeEnd = size - Size.Byte * i;
            var sourceSizeEnd = sourceSize - Size.Byte * i;

            if(sourceSizeEnd.IsZero)
                i--;
            else if(sizeEnd.IsZero)
                return;
            else
            {
                var bitsToByte = (Size.Byte - sizeEnd.Min(sourceSizeEnd)).ToInt();
                data[i] = (byte) ((sbyte) ((sbyte) source[i] << bitsToByte) >> bitsToByte);
            }

            if(i == -1)
                return;

            if((sbyte) data[i] >= 0)
                return;

            for(i++; i < size.ByteCount; i++)
                unchecked
                {
                    data[i] = (byte) -1;
                }
        }

        public byte Byte(int index)
        {
            if(index < _data.Length)
                return _data[index];
            if(_data.Length == 0 || _data[_data.Length - 1] < 0x80)
                return 0;
            return 0xff;
        }

        [DisableDump]
        BigInteger AsInteger => _dataCache.Value;

        public static BitsConst Convert(int bytes, byte[] data, int position)
            => new BitsConst(bytes, data, position);

        static int DataSize(Size size)
        {
            if(size.IsZero)
                return 0;
            return (size - 1) / SegmentBits + 1;
        }

        public static BitsConst None() => new BitsConst(Size.Zero);

        public static BitsConst Convert(long value) => new BitsConst(value);

        public static BitsConst Convert(string value)
            => new BitsConst(System.Convert.ToInt64(value));

        public static BitsConst ConvertAsText(string value)
        {
            Tracer.Assert(Marshal.SizeOf(value[0].GetType()) == 1);
            return new BitsConst(value.Length, value.Select(c => (byte) c).ToArray(), 0);
        }

        public static BitsConst Convert(string target, int @base)
        {
            long result = 0;
            for(var i = 0; i < target.Length; i++)
            {
                var digit = Digits.IndexOf
                    (target.Substring(i, 1), StringComparison.InvariantCultureIgnoreCase);
                Tracer.Assert(digit >= 0 && digit < @base);
                result = result * @base + digit;
            }
            return Convert(result);
        }

        public static int PlusSize(int left, int right) => Math.Max(left, right) + 1;

        public static int MultiplySize(int left, int right) => left + right - 1;

        internal static int DivideSize(int left, int right) => Math.Max(0, left - right) + 2;

        public BitsConst Resize(Size size) => new BitsConst(this, size);

        public BitsConst ByteResize(int size) => Resize(SegmentBits * size);

        public BitsConst ShiftDown(Size size)
        {
            Tracer.Assert
                (SlagBits(Size).IsZero, () => "Size of object is not byte aligned: " + Dump());
            Tracer.Assert
                (SlagBits(size).IsZero, () => "Target size is not byte aligned: " + size.Dump());

            var bytes = size.ByteCount;
            return Convert(_data.Length - bytes, _data, bytes);
        }

        public BitsConst Multiply(BitsConst right, Size size)
        {
            if(!(Marshal.SizeOf(typeof(long)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed
                    (
                        @"sizeof(Int64)*8 >= size.ToInt()",
                        () => "right=" + right + ";size=" + size.Dump());
            return Convert(ToInt64() * right.ToInt64()).Resize(size);
        }

        public BitsConst Multiply(int right, Size size) => Convert(ToInt64() * right).Resize(size);

        [UsedImplicitly]
        public BitsConst Star(BitsConst right, Size size) => Multiply(right, size);

        [UsedImplicitly]
        public BitsConst Slash(BitsConst right, Size size) => Divide(right, size);

        public BitsConst Divide(BitsConst right, Size size)
        {
            if(!(Marshal.SizeOf(typeof(long)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed
                    (
                        @"sizeof(Int64)*8 >= size.ToInt()",
                        () => "right=" + right + ";size=" + size.Dump());
            return Convert(ToInt64() / right.ToInt64()).Resize(size);
        }

        public BitsConst BytePlus(BitsConst left, int bytes) => Plus(left, SegmentBits * bytes);

        public BitsConst Plus(BitsConst right, Size size)
        {
            var xResult = new BitsConst(this, size);
            var yResult = new BitsConst(right, size);
            xResult.AddAndKeepSize(yResult);
            return xResult;
        }

        [UsedImplicitly]
        public BitsConst Minus(BitsConst right, Size size) => Plus(right * -1, size);

        [UsedImplicitly]
        public BitsConst Equal(BitsConst right, Size size)
            => ToBitsConst(AsInteger == right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst Greater(BitsConst right, Size size)
            => ToBitsConst(AsInteger > right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst GreaterEqual(BitsConst right, Size size)
            => ToBitsConst(AsInteger >= right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst Less(BitsConst right, Size size)
            => ToBitsConst(AsInteger < right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst LessEqual(BitsConst right, Size size)
            => ToBitsConst(AsInteger <= right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst LessGreater(BitsConst right, Size size)
            => ToBitsConst(AsInteger != right.AsInteger, size);

        [UsedImplicitly]
        public BitsConst MinusPrefix(Size size) => Multiply(-1, size);

        static BitsConst ToBitsConst(bool value, Size size) => new BitsConst(value ? -1 : 0, size);

        public BitsConst Concat(BitsConst other)
        {
            Size.AssertAlignedSize(SegmentAlignBits);
            var result = new BitsConst(Size + other.Size);
            DataHandler.MoveBytes(DataSize(Size), result._data, 0, _data, 0);
            DataHandler.MoveBytes
                (DataSize(other.Size), result._data, DataSize(Size), other._data, 0);
            return result;
        }

        public void PrintNumber(BitsConst radix, IOutStream outStream)
        {
            var r = radix.ToInt64();
            if(radix.Size.IsZero)
                r = 10;
            var left = ToString((int) r);

            outStream.AddData(left);
        }

        public void PrintNumber(IOutStream outStream) => PrintNumber(None(), outStream);

        public void PrintText(Size itemSize, IOutStream outStream)
            => outStream.AddData(ToString(itemSize));

        public string ToString(Size itemSize)
        {
            Tracer.Assert(itemSize == SegmentBits);
            return new string(_data.Select(c => (char) c).ToArray());
        }

        string ToString(int radix)
        {
            if(radix <= 0 || radix > Digits.Length)
                Tracer.AssertionFailed("radix <= 0 || radix > " + Digits.Length, radix.ToString);

            if(IsZero)
                return "0";
            if(IsNegative)
                return "-" + (0 - this).ToString(radix);

            var left = this / radix;
            var right = this - left * radix;
            var digit = Digits[(int) right.ToInt64()].ToString();
            if(left.IsZero)
                return digit;
            return left.ToString(radix) + digit;
        }

        public override string ToString() => DumpValue();
        protected override string GetNodeDump() => base.GetNodeDump() + " " + ToString();

        public unsafe long ToInt64()
        {
            var sizeInt64 = Marshal.SizeOf(typeof(long));
            if(!(64 >= Size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", () => "size=" + Size.Dump());

            var x64 = ByteResize(sizeInt64);
            fixed(byte* dataPtr = &x64._data[0])
                return *(long*) dataPtr;
        }

        public unsafe int ToInt32()
        {
            var sizeInt32 = Marshal.SizeOf(typeof(int));
            if(!(64 >= Size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", () => "size=" + Size.Dump());

            var x32 = ByteResize(sizeInt32);
            fixed(byte* dataPtr = &x32._data[0])
                return *(int*) dataPtr;
        }

        public string DumpValue()
        {
            if(Size.IsZero)
                return "0[0]";
            var digits = Size.ToInt() < 8 ? ToBitString() : ToHexString();
            var result = digits.Quote() + "[";
            result += Size.Dump();
            result += "]";
            return result;
        }

        string ToHexString()
        {
            var value = "";
            var n = _data.Length;
            for(var i = 0; i < n; i++)
                if(i < 2 || i >= n - 2 || n == 5)
                    value = HexDump(_data[i]) + value;
                else if(i == 3)
                    value = "..." + value;
            return value;
        }

        static string HexDump(byte left)
        {
            var result = "";
            result += Digits[(left >> 4) & 0xf];
            result += Digits[left & 0xf];
            return result;
        }

        string ToBitString()
        {
            var result = "";
            for(var i = Size.Create(0); i < Size; i += 1)
                result = "01"[GetBit(i)] + result;
            return result;
        }

        int GetBit(Size i)
        {
            var b = i / SegmentBits;
            var p = (i % SegmentBits).ToInt();
            return (_data[b] >> p) & 1;
        }

        public static BitsConst operator +(BitsConst left, BitsConst right)
            => left.Plus(right, PlusSize(left.Size, right.Size));

        static Size PlusSize(Size size, Size size1)
            => Size.Create(PlusSize(size.ToInt(), size1.ToInt()));

        public static BitsConst Add(BitsConst left, BitsConst right)
            => left.Plus(right, PlusSize(left.Size, right.Size));

        public static BitsConst operator -(BitsConst left, BitsConst right)
            => left.Plus(right * -1, PlusSize(left.Size, right.Size));

        public static BitsConst operator -(int left, BitsConst right) => Convert(left) - right;

        public static BitsConst Subtract(BitsConst left, BitsConst right)
            => left.Plus(right * -1, PlusSize(left.Size, right.Size));

        public static BitsConst Subtract(int left, BitsConst right) => Convert(left) - right;

        public static BitsConst operator *(BitsConst left, BitsConst right)
            => left.Multiply(right, MultiplySize(left.Size, right.Size));

        static Size MultiplySize(Size left, Size right)
            => Size.Create(MultiplySize(left.ToInt(), right.ToInt()));

        static Size DivideSize(Size left, Size right)
            => Size.Create(DivideSize(left.ToInt(), right.ToInt()));

        public static BitsConst operator *(BitsConst left, int right) => left * Convert(right);

        public static BitsConst Multiply(BitsConst left, BitsConst right)
            => left.Multiply(right, MultiplySize(left.Size, right.Size));

        public static BitsConst Multiply(BitsConst left, int right) => left * Convert(right);

        public static BitsConst operator /(BitsConst left, BitsConst right)
            => left.Divide(right, DivideSize(left.Size, right.Size));

        public static BitsConst operator /(BitsConst left, int right) => left / Convert(right);

        public static BitsConst Divide(BitsConst left, BitsConst right)
            => left.Divide(right, DivideSize(left.Size, right.Size));

        public static BitsConst Divide(BitsConst left, int right) => left / Convert(right);

        public override bool Equals(object obj)
        {
            var left = obj as BitsConst;
            if(left == null)
                return false;
            if(Size != left.Size)
                return false;
            return _data == left._data;
        }

        public override int GetHashCode() => _data.GetHashCode();

        public BitsConst Access(Size start, Size size)
        {
            NotImplementedMethod(start, size);
            return this;
        }

        public bool? Access(Size start)
        {
            if(start.IsNegative)
                return null;
            if(start >= Size)
                return null;
            return GetBit(start) != 0;
        }

        public void CopyTo(byte[] data, int index) => _data.CopyTo(data, index);

        public static BitsConst Convert(byte[] value) => new BitsConst(value.Length, value, 0);

        void AddAndKeepSize(BitsConst left)
        {
            short carry = 0;
            for(var i = 0; i < _data.Length; i++)
            {
                carry += _data[i];
                carry += left._data[i];
                _data[i] = (byte) carry;
                carry /= (short) SegmentValues;
            }
            return;
        }

        public string CodeDump() => ToInt64().ToString();

        [UnitTest]
        public sealed class Test
        {
            [UnitTest]
            public void All()
            {
                Tracer.FlaggedLine("Position of method tested", FilePositionTag.Test);
                Tracer.Assert(Convert(0).ToInt64() == 0);
                Tracer.Assert(Convert(11).ToInt64() == 11);
                Tracer.Assert(Convert(-111).ToInt64() == -111);

                Tracer.Assert((Convert(10) + Convert(1)).ToInt64() == 11);
                Tracer.Assert((Convert(10) + Convert(-1)).ToInt64() == 9);
                Tracer.Assert((Convert(1) + Convert(-10)).ToInt64() == -9);

                Tracer.Assert((Convert(1111) + Convert(-10)).ToInt64() == 1101);
                Tracer.Assert((Convert(1111) + Convert(-1110)).ToInt64() == 1);
                Tracer.Assert((Convert(1111) + Convert(-1112)).ToInt64() == -1);

                Tracer.Assert(Convert("0").ToString(10) == "0", () => Convert("0").ToString(10));
                Tracer.Assert(Convert("1").ToString(10) == "1", () => Convert("1").ToString(10));
                Tracer.Assert(Convert("21").ToString(10) == "21", () => Convert("21").ToString(10));
                Tracer.Assert(Convert("321").ToString(10) == "321");
                Tracer.Assert(Convert("4321").ToString(10) == "4321");
                Tracer.Assert(Convert("54321").ToString(10) == "54321");
                Tracer.Assert(Convert("654321").ToString(10) == "654321");
                Tracer.Assert(Convert("-1").ToString(10) == "-1");
                Tracer.Assert(Convert("-21").ToString(10) == "-21");
                Tracer.Assert(Convert("-321").ToString(10) == "-321");
                Tracer.Assert(Convert("-4321").ToString(10) == "-4321");
                Tracer.Assert(Convert("-54321").ToString(10) == "-54321");
                Tracer.Assert(Convert("-654321").ToString(10) == "-654321");
            }

            [UnitTest]
            public void Resize()
            {
                Tracer.FlaggedLine("Position of method tested", FilePositionTag.Test);
                Tracer.Assert(Convert("100").Resize(Size.Create(6)).ToString(10) == "-28");

                Tracer.Assert(Convert("1").Resize(Size.Create(1)).ToString(10) == "-1");
                Tracer.Assert(Convert("1").Resize(Size.Create(2)).ToString(10) == "1");
                Tracer.Assert(Convert("1").Resize(Size.Create(3)).ToString(10) == "1");

                Tracer.Assert(Convert("2").Resize(Size.Create(1)).ToString(10) == "0");
                Tracer.Assert
                    (
                        Convert("2").Resize(Size.Create(2)).ToString(10) == "-2",
                        () => Convert("2").Resize(Size.Create(2)).ToString(10));
                Tracer.Assert(Convert("2").Resize(Size.Create(3)).ToString(10) == "2");

                Tracer.Assert
                    (
                        Convert("-4").Resize(Size.Create(32)).ToString(10) == "-4",
                        () => Convert("-4").Resize(Size.Create(32)).ToString(10));
                Tracer.Assert
                    (
                        Convert("-4095").Resize(Size.Create(8)).ToString(10) == "1",
                        () => Convert("-4095").Resize(Size.Create(32)).ToString(10));
            }
        }

        internal string ByteSequence(Size size = null)
        {
            var result = "";
            for(var i = 0; i < (size ?? Size).ByteCount; i++)
            {
                result += ", ";
                result += Byte(i);
            }
            return result;
        }

        internal BitsConst BitArrayBinaryOp(string operation, Size size, BitsConst right)
        {
            var methodInfo = typeof(BitsConst).GetMethod(operation);
            if(methodInfo == null)
                throw new MissingMethodException(operation);
            return (BitsConst) methodInfo.Invoke(this, new object[] {right, size});
        }

        internal BitsConst BitArrayPrefixOp(string operation, Size size)
        {
            var methodInfo = typeof(BitsConst).GetMethod(operation + "Prefix");
            if(methodInfo == null)
                throw new MissingMethodException(operation);
            return (BitsConst) methodInfo.Invoke(this, new object[] {size});
        }

        sealed class MissingMethodException : Exception
        {
            [EnableDump]
            readonly string _operation;

            public MissingMethodException(string operation)
            {
                _operation = operation;
                Tracer.ThrowAssertionFailed("", () => Tracer.Dump(this), 1);
            }
        }
    }
}