using System;
using System.Collections;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using NUnit.Framework;
using Reni.FeatureTest;

namespace Reni
{
    /// <summary>
    /// Compiler visitor category that contains the size of any sytax element
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    public class Size : ReniObject
    {
        static Hashtable _values = new Hashtable();
        int _data;
        private Size(int x)
        {
            _data = x;   
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
		static public Size Create(int x)
		{
			Size result = (Size) _values[x];
			if(result == null)
			{
				result = new Size(x);
				_values[x] = result;
			};
			return result;
		}
        /// <summary>
        /// Sequence representation of value
        /// </summary>
        /// <returns></returns>
		public override string Dump(){return _data.ToString();}

        /// <summary>
        /// asis
        /// </summary>
        public bool IsZero
        {
            get { return _data == 0; }
        }

        /// <summary>
        /// Aligns the object to alignBis bits
        /// </summary>
        /// <param name="alignBits"></param>
        /// <returns></returns>
        public Size Align(int alignBits)
        {
            int result = SizeToPacketCount(alignBits) << alignBits;
            if(result == _data)
                return this;
            return Create(result);
        }
        /// <summary>
        /// Convert size into packets by use of align bits. 
        /// </summary>
        /// <param name="alignBits"></param>
        /// <returns></returns>
        public int SizeToPacketCount(int alignBits)
        {
            Tracer.Assert(!IsPending);
            return ((_data-1) >> alignBits)+1;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 25.01.2007 23:19
        public bool IsPending { get { return _data == -1; } }

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
            int result = SizeToPacketCount(alignBits);
            if ((result << alignBits) == _data)
                return result;
            NotImplementedMethod(alignBits);
            
            throw new NotAlignableException(this, alignBits);
        }

        /// <summary>
        /// Converts object to byte count, throws an expetion if not aligned .
        /// </summary>
        /// <value>The save byte count.</value>
        /// created 10.10.2006 01:13
        public int SaveByteCount { get { return SaveSizeToPacketCount(3); } }

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
            return Size.Create(_data % x._data);
        }
        /// <summary>
		/// Delegate operation to data field
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
        static public bool operator <(Size x, Size y) { return x.LessThan(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public bool operator >(Size x, Size y) { return y.LessThan(x); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator -(Size x, Size y) { return x.Minus(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator -(Size x, int y) { return x.Minus(y); }
        /// <summary>
		/// Delegate operation to data field
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static public Size operator +(int x, Size y) { return y.Plus(x); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size Add(int x, Size y) { return y.Plus(x); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator +(Size x, int y) { return x.Plus(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size Add(Size x, int y) { return x.Plus(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator +(Size x, Size y) { return x.Plus(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size Add(Size x, Size y) { return x.Plus(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator *(Size x, int y) { return x.Times(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator *(int x, Size y) { return y.Times(x); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator /(Size x, int y) { return x.Divide(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public int operator /(Size x, Size y) { return x.Divide(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size operator %(Size x, Size y) { return x.Modulo(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size Multiply(Size x, int y) { return x.Times(y); }
        /// <summary>
        /// Delegate operation to data field
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public Size Multiply(int x, Size y) { return y.Times(x); }

        private Size Plus(int y) { return Create(_data + y); }
        private Size Times(int y) { return Create(_data * y); }
        private Size Minus(int y) { return Create(_data - y); }
        private Size Divide(int y) { return Create(_data / y); }
        private Size Plus(Size y) { return Create(_data + y._data); }
        private int Divide(Size y) { return _data / y._data; }
        private Size Minus(Size y) { return Create(_data - y._data); }
		
        /// <summary>
        /// Compares to values (don't know, how to override the default compare operator in a natural way)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equ(int other){return _data == other;}
        /// <summary>
        /// Compares to values (don't know, how to override the default compare operator in a natural way)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equ(Size other){return other.Equ(_data);}

        /// <summary>
        /// Return the maximum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Size Max(Size x)
        {
            if(_data > x._data)
                return this;
            else
                return x;
        }
        /// <summary>
        /// Return the minimum
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Size Min(Size x)
        {
            if (_data < x._data)
                return this;
            else
                return x;
        }
        /// <summary>
		/// Return the zero size
		/// </summary>
    	static public Size Zero { get { return Create(0); } }
        /// <summary>
        /// Gets the byte size.
        /// </summary>
        /// <value>The byte.</value>
        /// created 17.01.2007 21:32
        public static Size Byte { get { return Create(1).ByteAlignedSize; } }


        /// <summary>
        /// Gets a value indicating whether this instance is positive.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is positive; otherwise, <c>false</c>.
        /// </value>
        /// [created 09.07.2006 14:22]
        public bool IsPositive { get { return _data > 0; } }

        /// <summary>
        /// Sizes to byte count.
        /// </summary>
        /// <returns></returns>
        /// [created 06.06.2006 01:15]
        public int ByteCount
        {
            get
            {
                return SizeToPacketCount(3);
            }
        }

        /// <summary>
        /// Special bytes that are required for this size.
        /// </summary>
        /// <value>The size of the byte aligned.</value>
        /// created 15.10.2006 13:26
        public Size ByteAlignedSize
        {
            get
            {
                return NextPacketSize(3);
            }
        }

        /// <summary>
        /// Gets the pseudo size for pending.requests
        /// </summary>
        /// <value>The pending.</value>
        /// created 24.01.2007 21:43
        static public Size Pending { get { return Create(-1); } }

        /// <summary>
        /// Toes the type of the C code byte.
        /// </summary>
        /// <returns></returns>
        /// [created 18.07.2006 23:03]
        public string ToCCodeByteType()
        {
            return "byte<" + ByteCount+ ">";
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

        [TestFixture]
        [Category(CompilerTest.Worked)]
        class Tests
        {
            [Test]
            [Category(CompilerTest.Worked)]
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

            private static void TestNextPacketSize(int x, int b)
            {
                Size xs = Create(x);
                Tracer.Assert(xs.NextPacketSize(3)==Create(b));
            }
        }

        ///<summary>
        ///Returns a <see cref="T:System.Sequence"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Sequence"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            return ToInt().ToString();
        }
    }

    internal class NotAlignableException : Exception
    {
        private readonly Size _size;
        private readonly int _bits;

        public NotAlignableException(Size size, int bits)
            : base(size.Dump() + " cannot be aligned to "+bits + " bits.")
        {
            _size = size;
            _bits = bits;
        }
    }

    /// <summary>
	/// Array of size objects
	/// </summary>
	sealed public class SizeArray: List<Size>
	{

		/// <summary>
		/// Default dump of data
		/// </summary>
		/// <returns></returns>
		public string DumpData()
		{
			string result = "(";
			for(int i=0; i<Count; i++)
			{
				if(i>0)
					result += ",";
				result += this[i].ToString();
			}
			return result+")";
		}

		/// <summary>
		/// obtain size
		/// </summary>
		public Size Size
		{
			get
			{
				Size result = Size.Create(0);
				for(int i=0; i<Count; i++)
					result += this[i];
				return result;
			}
		}
	}
}
