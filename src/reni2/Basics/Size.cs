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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using JetBrains.Annotations;

namespace Reni.Basics
{
    [AdditionalNodeInfo("DebuggerDumpString")]
    [DebuggerDisplay("{NodeDump,nq}")]
    internal sealed class Size : ReniObject, IIconKeyProvider, IComparable<Size>
    {
        private static readonly Hashtable _values = new Hashtable();
        private readonly int _value;
        private static int _nextObjectId;

        private Size(int value)
            : base(_nextObjectId++) { _value = value; }

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

        protected override string Dump(bool isRecursion) { return DumpShort(); }

        internal override string DumpShort() { return _value.ToString(); }

        public Size Align(int alignBits)
        {
            var result = SizeToPacketCount(alignBits) << alignBits;
            if(result == _value)
                return this;
            return Create(result);
        }

        public int SizeToPacketCount(int alignBits) { return ((_value - 1) >> alignBits) + 1; }

        public Size NextPacketSize(int alignBits) { return Create(SizeToPacketCount(alignBits) << alignBits); }

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

        public int ToInt() { return _value; }

        private bool LessThan(Size x) { return _value < x._value; }

        private Size Modulo(Size x) { return Create(_value%x._value); }

        public static bool operator <(Size x, Size y) { return x.LessThan(y); }

        public static bool operator >(Size x, Size y) { return y.LessThan(x); }

        public static bool operator <=(Size x, Size y) { return !(x > y); }

        public static bool operator >=(Size x, Size y) { return !(x < y); }

        public static Size operator -(Size x, Size y) { return x.Minus(y); }

        public static Size operator -(Size x, int y) { return x.Minus(y); }

        public static Size operator +(int x, Size y) { return y.Plus(x); }

        public static Size Add(int x, Size y) { return y.Plus(x); }

        public static Size operator +(Size x, int y) { return x.Plus(y); }

        public static Size Add(Size x, int y) { return x.Plus(y); }

        public static Size operator +(Size x, Size y) { return x.Plus(y); }

        public static Size Add(Size x, Size y) { return x.Plus(y); }

        public static Size operator *(Size x, int y) { return x.Times(y); }

        public static Size operator *(int x, Size y) { return y.Times(x); }

        public static Size operator /(Size x, int y) { return x.Divide(y); }

        public static int operator /(Size x, Size y) { return x.Divide(y); }

        public static Size operator %(Size x, Size y) { return x.Modulo(y); }

        public static Size Multiply(Size x, int y) { return x.Times(y); }

        public static Size Multiply(int x, Size y) { return y.Times(x); }

        private Size Plus(int y) { return Create(_value + y); }

        private Size Times(int y) { return Create(_value*y); }

        private Size Minus(int y) { return Create(_value - y); }

        private Size Divide(int y) { return Create(_value/y); }

        private Size Plus(Size y) { return Create(_value + y._value); }

        private int Divide(Size y) { return _value/y._value; }

        private Size Minus(Size y) { return Create(_value - y._value); }

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

        public string ToCCodeByteType() { return "byte<" + ByteCount + ">"; }

        [UsedImplicitly]
        public string CodeDump() { return ByteCount.ToString(); }

        public int CompareTo(Size other) { return LessThan(other) ? -1 : (other.LessThan(this) ? 1 : 0); }

        public override string ToString() { return ToInt().ToString(); }

        [TestFixture]
        private sealed class Tests
        {
            private static void TestNextPacketSize(int x, int b)
            {
                var xs = Create(x);
                Tracer.Assert(xs.NextPacketSize(BitsConst.SegmentAlignBits) == Create(b));
            }

            [Test]
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

        internal string FormatForView() { return ToString() + " " + ToCCodeByteType(); }

        /// <summary>
        ///     Gets the icon key.
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
    internal sealed class SizeArray : List<Size>
    {
        /// <summary>
        ///     obtain size
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