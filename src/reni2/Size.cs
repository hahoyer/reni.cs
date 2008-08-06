using System;
using System.Collections;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using NUnit.Framework;
using Reni.FeatureTest;

namespace Reni
{
    /// <summary>
    /// Compiler visitor category that contains the size of any sytax element
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    [Serializable]
    internal class Size : ReniObject, IIconKeyProvider
    {
        private static readonly Hashtable _values = new Hashtable();
        private readonly int _data;

        private Size(int x)
        {
            _data = x;
        }

        /// <summary>
        /// asis
        /// </summary>
        public bool IsZero { get { return _data == 0; } }
    public bool IsPending { get { return _data == -1; } }
        public int SaveByteCount { get { return SaveSizeToPacketCount(3); } }
        public static Size Zero { get { return Create(0); } }
        public static Size Byte { get { return Create(1).ByteAlignedSize; } }
        public bool IsPositive { get { return _data > 0; } }
        public int ByteCount { get { return SizeToPacketCount(3); } }
        public Size ByteAlignedSize { get { return NextPacketSize(3); } }
        public static Size Pending { get { return Create(-1); } }

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

        public override string Dump()
        {
            return _data.ToString();
        }

        public Size Align(int alignBits)
        {
            var result = SizeToPacketCount(alignBits) << alignBits;
            if(result == _data)
                return this;
            return Create(result);
        }

        public int SizeToPacketCount(int alignBits)
        {
            Tracer.Assert(!IsPending);
            return ((_data - 1) >> alignBits) + 1;
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
        public int SaveSizeToPacketCount(int alignBits)
        {
            var result = SizeToPacketCount(alignBits);
            if((result << alignBits) == _data)
                return result;
            NotImplementedMethod(alignBits);

            throw new NotAlignableException(this, alignBits);
        }

        /// <summary>
        /// asis
        /// </summary>
        /// <returns></returns>
        public int ToInt()
        {
            return _data;
        }

        private bool LessThan(Size x)
        {
            return _data < x._data;
        }

        private Size Modulo(Size x)
        {
            return Create(_data%x._data);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator <(Size x, Size y)
        {
            return x.LessThan(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size operator -(Size x, Size y)
        {
            return x.Minus(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size operator -(Size x, int y)
        {
            return x.Minus(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size operator *(Size x, int y)
        {
            return x.Times(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size operator *(int x, Size y)
        {
            return y.Times(x);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Size operator /(Size x, int y)
        {
            return x.Divide(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int operator /(Size x, Size y)
        {
            return x.Divide(y);
        }

        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
            return Create(_data + y);
        }

        private Size Times(int y)
        {
            return Create(_data*y);
        }

        private Size Minus(int y)
        {
            return Create(_data - y);
        }

        private Size Divide(int y)
        {
            return Create(_data/y);
        }

        private Size Plus(Size y)
        {
            return Create(_data + y._data);
        }

        private int Divide(Size y)
        {
            return _data/y._data;
        }

        private Size Minus(Size y)
        {
            return Create(_data - y._data);
        }

        /// <summary>
        /// Compares to values (don't know, how to override the default compare operator in a natural way)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equ(int other)
        {
            return _data == other;
        }

        /// <summary>
        /// Compares to values (don't know, how to override the default compare operator in a natural way)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equ(Size other)
        {
            return other.Equ(_data);
        }

        /// <summary>
        /// Return the maximum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Size Max(Size x)
        {
            if(_data > x._data)
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
            if(_data < x._data)
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

        public override string ToString()
        {
            return ToInt().ToString();
        }

        [TestFixture, Category(CompilerTest.Worked)]
        private class Tests
        {
            private static void TestNextPacketSize(int x, int b)
            {
                var xs = Create(x);
                Tracer.Assert(xs.NextPacketSize(3) == Create(b));
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
    }

    internal class NotAlignableException : Exception
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