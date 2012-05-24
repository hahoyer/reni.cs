// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using System.Numerics;
using System.Runtime.InteropServices;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.UnitTest;
using JetBrains.Annotations;
using Reni.Runtime;

namespace Reni.Basics
{
    [DumpToString]
    sealed class BitsConst : ReniObject
    {
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal const int SegmentAlignBits = 3;
        static int _nextObjectId;

        readonly Size _size;
        readonly byte[] _data;
        readonly SimpleCache<BigInteger> _dataCache;

        BitsConst(Size size)
            : base(_nextObjectId++)
        {
            _size = size;
            _data = CreateDataArray();
            _dataCache = new SimpleCache<BigInteger>(() => new BigInteger(_data));
            StopByObjectId(-7);
        }

        BitsConst(Int64 value, Size size)
            : this(new BitsConst(value), size) { }

        BitsConst(Int64 value)
            : this(Size.Create(AutoSize(value))) { DataHandler.MoveBytes(DataSize(_size), _data, 0, value); }

        BitsConst(BitsConst value, Size size)
            : this(size) { MoveData(_data, Size, value._data, value.Size); }

        BitsConst(int bytes, byte[] value, int start)
            : this(Size.Create(bytes * 8)) { DataHandler.MoveBytes(DataSize(_size), _data, 0, value, start); }

        static Size SegmentBits { get { return Size.Create(1 << SegmentAlignBits); } }
        static int SegmentValues { get { return 1 << SegmentBits.ToInt(); } }
        static int MaxSegmentValue { get { return SegmentValues - 1; } }
        public static int BitSize(System.Type t) { return Marshal.SizeOf(t) << SegmentAlignBits; }
        public bool IsEmpty { get { return Size.IsZero; } }
        public Size Size { get { return _size; } }
        public int ByteCount { get { return DataSize(Size); } }
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

        public byte[] ToByteArray() { return (byte[]) _data.Clone(); }

        static Size SlagBits(Size size) { return SegmentBits * DataSize(size) - size; }

        byte[] CreateDataArray() { return new byte[DataSize(_size)]; }

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
        BigInteger AsInteger { get { return _dataCache.Value; } }

        public static BitsConst Convert(int bytes, byte[] data, int position) { return new BitsConst(bytes, data, position); }

        static int DataSize(Size size)
        {
            if(size.IsZero)
                return 0;
            return (size - 1) / SegmentBits + 1;
        }

        public static BitsConst None() { return new BitsConst(Size.Zero); }

        public static BitsConst Convert(Int64 value) { return new BitsConst(value); }

        public static BitsConst Convert(string value) { return new BitsConst(System.Convert.ToInt64(value)); }

        public static BitsConst ConvertAsText(string value)
        {
            Tracer.Assert(Marshal.SizeOf(value[0].GetType()) == 1);
            return new BitsConst(value.Length, value.Select(c => (byte) c).ToArray(), 0);
        }

        public static BitsConst Convert(string target, int @base)
        {
            Int64 result = 0;
            for(var i = 0; i < target.Length; i++)
            {
                var digit = Digits.IndexOf(target.Substring(i, 1), StringComparison.InvariantCultureIgnoreCase);
                Tracer.Assert(digit >= 0 && digit < @base);
                result = result * @base + digit;
            }
            return Convert(result);
        }

        public static int PlusSize(int left, int right) { return Math.Max(left, right) + 1; }

        public static int MultiplySize(int left, int right) { return left + right - 1; }

        internal static int DivideSize(int left, int right) { return Math.Max(0, left - right) + 2; }

        internal static int AutoSize(long value)
        {
            var size = 1;
            var xn = value >= 0 ? value : -value;
            for(Int64 upper = 1; xn >= upper; size++, upper *= 2)
                continue;
            return size;
        }

        public BitsConst Resize(Size size) { return new BitsConst(this, size); }

        public BitsConst ByteResize(int size) { return Resize(SegmentBits * size); }

        public BitsConst Multiply(BitsConst right, Size size)
        {
            if(!(Marshal.SizeOf(typeof(Int64)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed
                    (
                        @"sizeof(Int64)*8 >= size.ToInt()",
                        () => "right=" + right + ";size=" + size.Dump());
            return Convert(ToInt64() * right.ToInt64()).Resize(size);
        }

        public BitsConst Multiply(int right, Size size) { return Convert(ToInt64() * right).Resize(size); }

        [UsedImplicitly]
        public BitsConst Star(BitsConst right, Size size) { return Multiply(right, size); }

        [UsedImplicitly]
        public BitsConst Slash(BitsConst right, Size size) { return Divide(right, size); }

        public BitsConst Divide(BitsConst right, Size size)
        {
            if(!(Marshal.SizeOf(typeof(Int64)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed
                    (
                        @"sizeof(Int64)*8 >= size.ToInt()",
                        () => "right=" + right + ";size=" + size.Dump());
            return Convert(ToInt64() / right.ToInt64()).Resize(size);
        }

        public BitsConst BytePlus(BitsConst left, int bytes) { return Plus(left, SegmentBits * bytes); }

        public BitsConst Plus(BitsConst right, Size size)
        {
            var xResult = new BitsConst(this, size);
            var yResult = new BitsConst(right, size);
            xResult.AddAndKeepSize(yResult);
            return xResult;
        }

        [UsedImplicitly]
        public BitsConst Minus(BitsConst right, Size size) { return Plus(right * -1, size); }

        [UsedImplicitly]
        public BitsConst Equal(BitsConst right, Size size) { return ToBitsConst(AsInteger == right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst Greater(BitsConst right, Size size) { return ToBitsConst(AsInteger > right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst GreaterEqual(BitsConst right, Size size) { return ToBitsConst(AsInteger >= right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst Less(BitsConst right, Size size) { return ToBitsConst(AsInteger < right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst LessEqual(BitsConst right, Size size) { return ToBitsConst(AsInteger <= right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst LessGreater(BitsConst right, Size size) { return ToBitsConst(AsInteger != right.AsInteger, size); }

        [UsedImplicitly]
        public BitsConst MinusPrefix(Size size) { return Multiply(-1, size); }

        static BitsConst ToBitsConst(bool value, Size size) { return new BitsConst(value ? -1 : 0, size); }

        public BitsConst Concat(BitsConst other)
        {
            Size.AssertAlignedSize(SegmentAlignBits);
            var result = new BitsConst(Size + other.Size);
            DataHandler.MoveBytes(DataSize(Size), result._data, 0, _data, 0);
            DataHandler.MoveBytes(DataSize(other.Size), result._data, DataSize(Size), other._data, 0);
            return result;
        }

        public void PrintNumber(BitsConst radix, IOutStream outStream)
        {
            var r = radix.ToInt64();
            if(radix.Size.IsZero)
                r = 10;
            var left = ToString((int) r);

            outStream.Add(left);
        }

        public void PrintNumber(IOutStream outStream) { PrintNumber(None(), outStream); }
        public void PrintText(Size itemSize, IOutStream outStream) { outStream.Add(ToString(itemSize)); }

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
            var digit = (Digits[(int) right.ToInt64()]).ToString();
            if(left.IsZero)
                return digit;
            return left.ToString(radix) + digit;
        }

        public override string ToString() { return DumpValue(); }
        public override string NodeDump { get { return base.NodeDump + " " + ToString(); } }

        public unsafe Int64 ToInt64()
        {
            var sizeInt64 = Marshal.SizeOf(typeof(Int64));
            if(!(64 >= _size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", () => "size=" + _size.Dump());

            var x64 = ByteResize(sizeInt64);
            fixed(byte* dataPtr = &x64._data[0])
                return *((Int64*) dataPtr);
        }

        public unsafe int ToInt32()
        {
            var sizeInt32 = Marshal.SizeOf(typeof(Int32));
            if(!(64 >= _size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", () => "size=" + _size.Dump());

            var x32 = ByteResize(sizeInt32);
            fixed(byte* dataPtr = &x32._data[0])
                return *((Int32*) dataPtr);
        }

        public string DumpValue()
        {
            if(_size.IsZero)
                return "0[0]";
            var digits = _size.ToInt() < 8 ? ToBitString() : ToHexString();
            var result = digits.Quote() + "[";
            result += _size.Dump();
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
            for(var i = Size.Create(0); i < _size; i += 1)
                result = "01"[GetBit(i)] + result;
            return result;
        }

        int GetBit(Size i)
        {
            var b = i / SegmentBits;
            var p = (i % SegmentBits).ToInt();
            return (_data[b] >> p) & 1;
        }

        public static BitsConst operator +(BitsConst left, BitsConst right) { return left.Plus(right, PlusSize(left.Size, right.Size)); }

        static Size PlusSize(Size size, Size size1) { return Size.Create(PlusSize(size.ToInt(), size1.ToInt())); }

        public static BitsConst Add(BitsConst left, BitsConst right) { return left.Plus(right, PlusSize(left.Size, right.Size)); }

        public static BitsConst operator -(BitsConst left, BitsConst right) { return left.Plus(right * -1, PlusSize(left.Size, right.Size)); }

        public static BitsConst operator -(int left, BitsConst right) { return Convert(left) - right; }

        public static BitsConst Subtract(BitsConst left, BitsConst right) { return left.Plus(right * -1, PlusSize(left.Size, right.Size)); }

        public static BitsConst Subtract(int left, BitsConst right) { return Convert(left) - right; }

        public static BitsConst operator *(BitsConst left, BitsConst right) { return left.Multiply(right, MultiplySize(left.Size, right.Size)); }

        static Size MultiplySize(Size left, Size right) { return Size.Create(MultiplySize(left.ToInt(), right.ToInt())); }

        static Size DivideSize(Size left, Size right) { return Size.Create(DivideSize(left.ToInt(), right.ToInt())); }

        public static BitsConst operator *(BitsConst left, int right) { return left * Convert(right); }

        public static BitsConst Multiply(BitsConst left, BitsConst right) { return left.Multiply(right, MultiplySize(left.Size, right.Size)); }

        public static BitsConst Multiply(BitsConst left, int right) { return left * Convert(right); }

        public static BitsConst operator /(BitsConst left, BitsConst right) { return left.Divide(right, DivideSize(left.Size, right.Size)); }

        public static BitsConst operator /(BitsConst left, int right) { return left / Convert(right); }

        public static BitsConst Divide(BitsConst left, BitsConst right) { return left.Divide(right, DivideSize(left.Size, right.Size)); }

        public static BitsConst Divide(BitsConst left, int right) { return left / Convert(right); }

        public override bool Equals(object obj)
        {
            var left = obj as BitsConst;
            if(left == null)
                return false;
            if(Size != left.Size)
                return false;
            return _data == left._data;
        }

        public override int GetHashCode() { return _data.GetHashCode(); }

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

        public void CopyTo(byte[] data, int index) { _data.CopyTo(data, index); }

        public static BitsConst Convert(byte[] value) { return new BitsConst(value.Length, value, 0); }

        void AddAndKeepSize(BitsConst left)
        {
            Int16 carry = 0;
            for(var i = 0; i < _data.Length; i++)
            {
                carry += _data[i];
                carry += left._data[i];
                _data[i] = (byte) carry;
                carry /= (Int16) SegmentValues;
            }
            return;
        }

        public string CodeDump() { return ToInt64().ToString(); }

        [TestFixture]
        public sealed class Test
        {
            [Test]
            public void All()
            {
                Tracer.FlaggedLine(FilePositionTag.Test, "Position of method tested");
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

            [Test]
            public void Resize()
            {
                Tracer.FlaggedLine(FilePositionTag.Test, "Position of method tested");
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

        internal string ByteSequence(Size size)
        {
            var result = "";
            for(var i = 0; i < size.ByteCount; i++)
            {
                if(i > 0)
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
                Tracer.ThrowAssertionFailed(1, "", () => Tracer.Dump(this));
            }
        }
    }
}