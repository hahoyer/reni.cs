using HWClassLibrary.TreeStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest;

namespace Reni
{
    /// <summary>
    /// Compiler visitor category that contains the size of any sytax element
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    [Serializable]
    internal sealed class Size : ReniObject, IIconKeyProvider, IComparable<Size>
    {
        private static readonly Hashtable _values = new Hashtable();
        private readonly int _value;
        static private int _nextObjectId;

        private Size(int value): base(_nextObjectId++)
        {
            _value = value;
        }

        /// <summary>
        /// asis
        /// </summary>
        public bool IsZero { get { return _value == 0; } }
        public int SaveByteCount { get { return SaveSizeToPacketCount(BitsConst.SegmentAlignBits); } }
        public static Size Zero { get { return Create(0); } }
        public static Size Byte { get { return Create(1).ByteAlignedSize; } }
        public bool IsPositive { get { return _value > 0; } }
        public int ByteCount { get { return SizeToPacketCount(BitsConst.SegmentAlignBits); } }
        public Size ByteAlignedSize { get { return NextPacketSize(BitsConst.SegmentAlignBits); } }

        public static Size Create(int x)
        {
            var result = (Size) _values[x];
            if(result == null)
            {
                result = new Size(x);
                _values[x] = result;
            }
            return result;
        }

        protected override string Dump(bool isRecursion)
        {
            return _value.ToString();
        }

        public Size Align(int alignBits)
        {
            var result = SizeToPacketCount(alignBits) << alignBits;
            if(result == _value)
                return this;
            return Create(result);
        }

        public int SizeToPacketCount(int alignBits)
        {
            return ((_value - 1) >> alignBits) + 1;
        }

        /// <summary>
        /// Nexts the size of the packet.
        /// </summary>
        /// <param name="alignBits">The align bits.</param>
        /// <returns></returns>
        /// created 15.10.2006 13:24
        public Size NextPacketSize(int alignBits)
        {
            return Create(SizeToPacketCount(alignBits) << alignBits);
        }

        /// <summary>
        /// Convert size into packets by use of align bits, must not cut anything. 
        /// </summary>
        /// <param name="alignBits"></param>
        /// <returns></returns>
        internal void AssertAlignedSize(int alignBits)
        {
            var result = SizeToPacketCount(alignBits);
            if((result << alignBits) == _value)
                return;
            NotImplementedMethod(alignBits);

            throw new NotAlignableException(this, alignBits);
        }

        private int SaveSizeToPacketCount(int alignBits)
        {
            AssertAlignedSize(alignBits);
            return SizeToPacketCount(alignBits);
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <returns></returns>
        public int ToInt()
        {
            return _value;
        }

        private bool LessThan(Size x)
        {
            return _value < x._value;
        }

        private Size Modulo(Size x)
        {
            return Create(_value%x._value);
        }

        public static bool operator <(Size x, Size y)
        {
            return x.LessThan(y);
        }

        public static bool operator >(Size x, Size y)
        {
            return y.LessThan(x);
        }

        public static bool operator <=(Size x, Size y)
        {
            return !(x > y);
        }

        public static bool operator >=(Size x, Size y)
        {
            return !(x < y);
        }

        public static Size operator -(Size x, Size y)
        {
            return x.Minus(y);
        }

        public static Size operator -(Size x, int y)
        {
            return x.Minus(y);
        }

        public static Size operator +(int x, Size y)
        {
            return y.Plus(x);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size Add(int x, Size y)
        {
            return y.Plus(x);
        }

        public static Size operator +(Size x, int y)
        {
            return x.Plus(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size Add(Size x, int y)
        {
            return x.Plus(y);
        }

        public static Size operator +(Size x, Size y)
        {
            return x.Plus(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size Add(Size x, Size y)
        {
            return x.Plus(y);
        }

        public static Size operator *(Size x, int y)
        {
            return x.Times(y);
        }

        public static Size operator *(int x, Size y)
        {
            return y.Times(x);
        }

        public static Size operator /(Size x, int y)
        {
            return x.Divide(y);
        }

        public static int operator /(Size x, Size y)
        {
            return x.Divide(y);
        }

        public static Size operator %(Size x, Size y)
        {
            return x.Modulo(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size Multiply(Size x, int y)
        {
            return x.Times(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size Multiply(int x, Size y)
        {
            return y.Times(x);
        }

        private Size Plus(int y)
        {
            return Create(_value + y);
        }

        private Size Times(int y)
        {
            return Create(_value*y);
        }

        private Size Minus(int y)
        {
            return Create(_value - y);
        }

        private Size Divide(int y)
        {
            return Create(_value/y);
        }

        private Size Plus(Size y)
        {
            return Create(_value + y._value);
        }

        private int Divide(Size y)
        {
            return _value/y._value;
        }

        private Size Minus(Size y)
        {
            return Create(_value - y._value);
        }

        /// <summary>
        /// Return the maximum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Size Max(Size x)
        {
            if(_value > x._value)
                return this;
            return x;
        }

        /// <summary>
        /// Return the minimum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Size Min(Size x)
        {
            if(_value < x._value)
                return this;
            return x;
        }

        /// <summary>
        /// Toes the type of the C code byte.
        /// </summary>
        /// <returns></returns>
        /// [created 18.07.2006 23:03]
        public string ToCCodeByteType()
        {
            return "byte<" + ByteCount + ">";
        }

        /// <summary>
        /// Codes the dump.
        /// </summary>
        /// <returns></returns>
        /// created 23.09.2006 14:13
        public string CodeDump()
        {
            return ByteCount.ToString();
        }

        public int CompareTo(Size other) { return LessThan(other) ? -1 : (other.LessThan(this) ? 1 : 0); }

        public override string ToString()
        {
            return ToInt().ToString();
        }

        [TestFixture]
        private sealed class Tests
        {
            private static void TestNextPacketSize(int x, int b)
            {
                var xs = Create(x);
                Tracer.Assert(xs.NextPacketSize(BitsConst.SegmentAlignBits) == Create(b));
            }

            [Test, Category(CompilerTest.Worked)]
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
        }

        internal string FormatForView()
        {
            return ToString() + " " + ToCCodeByteType();
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Size"; } }

        internal bool IsNegative { get { return !(IsPositive || IsZero); } }

        internal Size Absolute
        {
            get
            {
                if(IsPositive)
                    return this;
                return this*-1;
            }
        }
    }

    internal sealed class NotAlignableException : Exception
    {
        internal readonly int Bits;
        internal readonly Size Size;

        public NotAlignableException(Size size, int bits)
            : base(size.Dump() + " cannot be aligned to " + bits + " bits.")
        {
            Size = size;
            Bits = bits;
        }
    }

    /// <summary>
    /// Array of size objects
    /// </summary>
    internal sealed class SizeArray : List<Size>
    {
        /// <summary>
        /// obtain size
        /// </summary>
        public Size Size
        {
            get
            {
                var result = Size.Create(0);
                for(var i = 0; i < Count; i++)
                    result += this[i];
                return result;
            }
        }

        /// <summary>
        /// Default dump of data
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