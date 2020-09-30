using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using JetBrains.Annotations;

namespace Reni.Basics
{
    [AdditionalNodeInfo("DebuggerDumpString")]
    [DebuggerDisplay("{NodeDump,nq}")]
    sealed class Size
        : DumpableObject
            , IIconKeyProvider
            , IComparable<Size>
            , IAggregateable<Size>
    {
        [UnitTest]
        sealed class Tests
        {
            [UnitTest]
            public void NextPacketSize()
            {
                TestNextPacketSize(0, 0);
                TestNextPacketSize(1, 8);
                TestNextPacketSize(2, 8);
                TestNextPacketSize(4, 8);
                TestNextPacketSize(6, 8);
                TestNextPacketSize(7, 8);
                TestNextPacketSize(8, 8);
                TestNextPacketSize(9, 16);
                TestNextPacketSize(15, 16);
                TestNextPacketSize(16, 16);
                TestNextPacketSize(17, 24);
            }

            static void TestNextPacketSize(int x, int b)
            {
                var xs = Create(x);
                Tracer.Assert(xs.NextPacketSize(BitsConst.SegmentAlignBits) == Create(b));
            }
        }

        static readonly Hashtable Values = new Hashtable();
        static int _nextObjectId;
        readonly int _value;

        Size(int value)
            : base(_nextObjectId++)
            => _value = value;

        public bool IsZero => _value == 0;

        public int SaveByteCount => SaveSizeToPacketCount(BitsConst.SegmentAlignBits);
        public static Size Zero => Create(0);
        public static Size Bit => Create(1);
        public static Size Byte => Bit.ByteAlignedSize;
        public bool IsPositive => _value > 0;
        public int ByteCount => SizeToPacketCount(BitsConst.SegmentAlignBits);
        public Size ByteAlignedSize => NextPacketSize(BitsConst.SegmentAlignBits);

        internal bool IsNegative => !(IsPositive || IsZero);

        internal Size Absolute
        {
            get
            {
                if(IsPositive)
                    return this;
                return this * -1;
            }
        }

        Size IAggregateable<Size>.Aggregate(Size other) => this + other;

        public int CompareTo(Size other) => LessThan(other)? -1 :
            other.LessThan(this)? 1 : 0;


        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey => "Size";

        public static Size Create(int x)
        {
            var result = (Size)Values[x];
            if(result == null)
            {
                result = new Size(x);
                Values[x] = result;
            }

            return result;
        }

        public Size Align(int alignBits)
        {
            var result = SizeToPacketCount(alignBits) << alignBits;
            if(result == _value)
                return this;
            return Create(result);
        }

        public int SizeToPacketCount(int alignBits) => ((_value - 1) >> alignBits) + 1;

        public Size NextPacketSize(int alignBits)
            => Create(SizeToPacketCount(alignBits) << alignBits);

        public int ToInt() => _value;

        public static bool operator <(Size x, Size y) => x.LessThan(y);

        public static bool operator >(Size x, Size y) => y.LessThan(x);

        public static bool operator <=(Size x, Size y) => !(x > y);

        public static bool operator >=(Size x, Size y) => !(x < y);

        public static Size operator -(Size x, Size y) => x.Minus(y);

        public static Size operator -(Size x, int y) => x.Minus(y);

        public static Size operator +(int x, Size y) => y.Plus(x);

        public static Size Add(int x, Size y) => y.Plus(x);

        public static Size operator +(Size x, int y) => x.Plus(y);

        public static Size Add(Size x, int y) => x.Plus(y);

        public static Size operator +(Size x, Size y) => x.Plus(y);

        public static Size Add(Size x, Size y) => x.Plus(y);

        public static Size operator *(Size x, int y) => x.Times(y);

        public static Size operator *(int x, Size y) => y.Times(x);

        public static Size operator /(Size x, int y) => x.Divide(y);

        public static int operator /(Size x, Size y) => x.Divide(y);

        public static Size operator %(Size x, Size y) => x.Modulo(y);

        public static Size Multiply(Size x, int y) => x.Times(y);

        public static Size Multiply(int x, Size y) => y.Times(x);

        public Size Max(Size x)
        {
            if(_value > x._value)
                return this;
            return x;
        }

        public Size Min(Size x)
        {
            if(_value < x._value)
                return this;
            return x;
        }

        public string ToCCodeByteType() => "byte<" + ByteCount + ">";

        [UsedImplicitly]
        public string CodeDump() => ByteCount.ToString();

        public override string ToString() => ToInt().ToString();

        protected override string Dump(bool isRecursion) => GetNodeDump();

        protected override string GetNodeDump() => _value.ToString();

        internal static Size AutoSize(long value)
        {
            var size = 1;
            var xn = value >= 0? value : -value;
            for(long upper = 1; xn >= upper; size++, upper *= 2) { }

            return Create(size);
        }

        internal void AssertAlignedSize(int alignBits)
        {
            var result = SizeToPacketCount(alignBits);
            if(result << alignBits == _value)
                return;
            NotImplementedMethod(alignBits);

            throw new NotAlignableException(this, alignBits);
        }

        internal string FormatForView() => ToString() + " " + ToCCodeByteType();

        int SaveSizeToPacketCount(int alignBits)
        {
            AssertAlignedSize(alignBits);
            return SizeToPacketCount(alignBits);
        }

        bool LessThan(Size x) => _value < x._value;

        Size Modulo(Size x) => Create(_value % x._value);

        Size Plus(int y) => Create(_value + y);

        Size Times(int y) => Create(_value * y);

        Size Minus(int y) => Create(_value - y);

        Size Divide(int y) => Create(_value / y);

        Size Plus(Size y) => Create(_value + y._value);

        int Divide(Size y) => _value / y._value;

        Size Minus(Size y) => Create(_value - y._value);
    }

    interface IIconKeyProvider
    {
        string IconKey { get; }
    }

    sealed class NotAlignableException : Exception
    {
        [EnableDump]
        internal readonly int Bits;

        [EnableDump]
        internal readonly Size Size;

        public NotAlignableException(Size size, int bits)
            : base(size.Dump() + " cannot be aligned to " + bits + " bits.")
        {
            Size = size;
            Bits = bits;
        }
    }

    /// <summary>
    ///     Array of size objects
    /// </summary>
    sealed class SizeArray : List<Size>
    {
        /// <summary>
        ///     obtain size
        /// </summary>
        public Size Size
        {
            get
            {
                var result = Size.Zero;
                for(var i = 0; i < Count; i++)
                    result += this[i];
                return result;
            }
        }

        /// <summary>
        ///     Default dump of data
        /// </summary>
        /// <returns></returns>
        public string DumpData()
        {
            var result = "(";
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    result += ",";
                result += this[i].ToString();
            }

            return result + ")";
        }
    }
}