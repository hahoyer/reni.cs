using System;
using System.Runtime.InteropServices;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.FeatureTest;
using Reni.Runtime;

namespace Reni
{
	/// <summary>
	/// Size and content are known at runtime
	/// </summary>
	[dumpToString]
	public class BitsConst : ReniObject
	{
        static private OutStream _outStream;
        
	    private Size _size;
        private byte[] _data;

	    private const int SegmentAlignBits = 3;
        private static Size SegmentBits { get { return Size.Create(1 << SegmentAlignBits); } }
        private static int SegmentValues { get { return 1 << SegmentBits.ToInt(); } }
        private static int MaxSegmentValue { get { return SegmentValues - 1; } }
        private static Size SlagBits(Size size) { return SegmentBits * DataSize(size) - size; }
	    
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private BitsConst(Size size)
        {
            _size = size;
            CreateDataArray();
        }

	    private void CreateDataArray()
	    {
	        _data = new byte[DataSize(_size)];
	    }

        private BitsConst(Int64 value)
            : this(AutoSize(value))
        {
            Data.MoveBytes(DataSize(_size), _data, 0, value);
        }

        private BitsConst(BitsConst value, Size size)
            : this(size)
        {
            MoveData(_data, Size, value._data, value.Size);
        }

	    private static void MoveData(byte[] data, Size size, byte[] source, Size sourceSize)
	    {
	        int i = 0;
	        for (int n = size.Min(sourceSize).ToInt() >> SegmentAlignBits; i < n; i++)
	            data[i] = source[i];

	        Size sizeEnd = size - Size.Byte*i;
	        Size sourceSizeEnd = sourceSize - Size.Byte*i;

	        if (sourceSizeEnd.IsZero)
	            i--;
	        else if (sizeEnd.IsZero)
	            return;
	        else
	        {
	            int bitsToByte = (Reni.Size.Byte - sizeEnd.Min(sourceSizeEnd)).ToInt();
	            data[i] = (byte) ((sbyte) ((sbyte) source[i] << bitsToByte) >> bitsToByte);
	        }

            if (i == -1)
                return;

            if ((sbyte)data[i] >= 0)
	            return;

	        for (i++; i < size.ByteCount; i++)
	            unchecked{data[i] = (byte) -1;}
	    }

        /// <summary>
        /// Bytes the specified index.
        /// </summary>
        /// <param name="index">The i.</param>
        /// <returns></returns>
        /// created 02.02.2007 00:46
        public byte Byte(int index)
        {
            if (index < _data.Length)
                return _data[index];
            if (_data.Length == 0 || _data[_data.Length - 1] < 0x80)
                return 0;
            return 0xff;
        }

        private BitsConst(int bytes, byte[] value, int start)
        {
            _size = Size.Create(bytes*8);
            CreateDataArray();
            Data.MoveBytes(DataSize(_size), _data, 0, value, start);
        }

        /// <summary>
        /// Converts the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="data">The data.</param>
        /// <param name="position">The posn.</param>
        /// <returns></returns>
        /// created 08.10.2006 20:33
        public static BitsConst Convert(int bytes, byte[] data, int position)
        {
            return new BitsConst(bytes, data, position);
        }

	    /// <summary>
        /// Check if has size of zero
        /// </summary>
        public bool IsEmpty { get { return Size.IsZero; } }

        static int DataSize(Size size)
        {
            if (size.IsZero)
                return 0;
            return (size - 1) / SegmentBits + 1;
        }

        /// <summary>
        /// Create an empty object of size 0
        /// </summary>
        /// <returns></returns>
        public static BitsConst None()
        {
            return new BitsConst(Reni.Size.Zero);
        }

        /// <summary>
        /// Create a BitsConst object from an int
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static BitsConst Convert(Int64 arg)
        {
            return new BitsConst(arg);
        }
        /// <summary>
        /// Create a BitsConst object from a string
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        public static BitsConst Convert(string left)
        {
            return new BitsConst(System.Convert.ToInt64(left));
        }

        /// <summary>
        /// Size of result in case of plus operation
        /// </summary>
        /// <param name="left">size of 1st operand</param>
        /// <param name="right">size of 2nd operand</param>
        /// <returns></returns>
        public static int PlusSize(int left, int right)
        {
            return Math.Max(left,right)+1;
        }
        /// <summary>
        /// Size of result in case of multiplication operation
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int MultiplySize(int left, int right)
        {
            return left + right - 1;
        }

        /// <summary>
        /// Size of result in case of division operation
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static int DivideSize(int left, int right)
        {
            return Math.Max(0,left - right) + 2;
        }

        private static Size AutoSize(Int64 value)
        {
            int size = 1;
            Int64 xn = value >= 0 ? value : -value;
            for (Int64 upper = 1; xn >= upper; size++, upper *= 2) 
                continue;
            return Size.Create(size);
        }

        /// <summary>
        /// The size in bits
        /// </summary>
        public Size Size { get { return _size; } }
	    /// <summary>
	    /// The size in bytes
	    /// </summary>
        public int ByteCount { get { return DataSize(Size); } }

	    /// <summary>
	    /// asis
	    /// </summary>
	    public bool IsZero
	    {
	        get
	        {
	            for(int i=0; i<_data.Length; i++)
	            {
	                if(_data[i] != 0)
	                    return false;
	            }
	            return true;
	        }
	    }

        private bool IsNegative
        {
            get
            {
                if(Size.IsZero)
                    return false;
                return GetBit(Size-1) != 0;
            }
        }

		/// <summary>
		/// Create an object with the same or cutted value and a different size. 
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public BitsConst Resize(Size size)
		{
			return new BitsConst(this, size);
		}

        /// <summary>
        /// Create an object with the same or cutted value and a different size. 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public BitsConst ByteResize(int size)
        {
            return Resize(SegmentBits*size);
        }
        /// <summary>
        /// asis
        /// </summary>
        /// <param name="left"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public BitsConst Multiply(BitsConst left, Size size)
		{
            
            if (!(Marshal.SizeOf(typeof(Int64)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed(@"sizeof(Int64)*8 >= size.ToInt()", "left=" + left + ";size=" + size.Dump());
            return Convert(ToInt64() * left.ToInt64()).Resize(size);
		}
        /// <summary>
        /// asis
        /// </summary>
        /// <param name="left"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public BitsConst Divide(BitsConst left, Size size)
        {
            if (!(Marshal.SizeOf(typeof(Int64)) * 8 >= size.ToInt()))
                Tracer.AssertionFailed(@"sizeof(Int64)*8 >= size.ToInt()", "left=" + left + ";size=" + size.Dump());
            return Convert(ToInt64() / left.ToInt64()).Resize(size);
        }

        /// <summary>
        /// Add two constants
        /// </summary>
        /// <param name="left"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public BitsConst BytePlus(BitsConst left, int bytes)
        {
            return Plus(left, SegmentBits*bytes);
        }
        /// <summary>
		/// Add two constants
		/// </summary>
		/// <param name="left"></param>
        /// <param name="size"></param>
		/// <returns></returns>
		public BitsConst Plus(BitsConst left, Size size)
		{
            BitsConst xResult = new BitsConst(this, size);
            BitsConst yResult = new BitsConst(left, size);
            xResult.AddAndKeepSize(yResult);
            return xResult;
		}

        /// <summary>
        /// Print a number
        /// </summary>
        /// <param name="radix"></param>
        public void PrintNumber(BitsConst radix)
        {
            long r = radix.ToInt64();
            if(radix.Size.IsZero)
                r = 10;
            string left = ToString((int)r);

            _outStream.Add(left);
        }
        /// <summary>
        /// Print a number
        /// </summary>
        public void PrintNumber()
        {
            PrintNumber(None());
        }
        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="radix"></param>
        /// <returns></returns>
	    public string ToString(int radix)
	    {
            if(radix <= 0 || radix > Digits.Length) Tracer.AssertionFailed("radix <= 0 || radix > "+Digits.Length.ToString(), radix.ToString());

            if(IsZero)
                return "0";
            if(IsNegative)
                return "-" + (0-this).ToString(radix);

            BitsConst left = this / radix;
            BitsConst right = this - left * radix;
	        string Digit = (Digits[(int)right.ToInt64()]).ToString();
            if(left.IsZero)
                return Digit;
            return left.ToString(radix) + Digit;
	    }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// created 09.02.2007 00:10
        /// 26.06.2007 22:42 on HAHOYER-DELL by h
		public override string ToString()
		{
			return DumpValue();
		}

        /// <summary>
        /// Convert to integer, if small enough
        /// </summary>
        /// <returns></returns>
        /// [created 06.09.2006 23:45]
        unsafe public Int64 ToInt64()
        {
            int sizeInt64 = Marshal.SizeOf(typeof(Int64));
            if (!(64 >= _size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", "size=" + _size.Dump());

            BitsConst x64 = ByteResize(sizeInt64);
            fixed (byte* dataPtr = &x64._data[0])
                return *((Int64*)dataPtr);
        }

        /// <summary>
        /// Convert to integer, if small enough
        /// </summary>
        /// <returns></returns>
        /// created 22.05.2007 00:23 on HAHOYER-DELL by hh
        unsafe public int ToInt32()
        {
            int sizeInt32 = Marshal.SizeOf(typeof(Int32));
            if (!(64 >= _size.ToInt()))
                Tracer.AssertionFailed(@"64 >= _size.ToInt()", "size=" + _size.Dump());

            BitsConst x32 = ByteResize(sizeInt32);
            fixed (byte* dataPtr = &x32._data[0])
                return *((Int32*)dataPtr);
        }
        /// <summary>
		/// Fomat size and value 
		/// </summary>
		/// <returns></returns>
		public string DumpValue()
        {
            string result = "[";
            result += _size.Dump();
            result += "bits]";
            if(_size.IsZero)
                return result + "0";

            if(_size.ToInt() < 8)
                return result + DumpAsBit();

            return result + ToHexString();
        }

	    private string ToHexString()
	    {
            string value = "";
			for(int i=0, n = _data.Length; i<n; i++)
			{
				if(i < 2 || i >= n-2 || n == 5)
					value = HexDump(_data[i]) + value;
				else if(i==3)
					value = "..."+value;
			}
			return value;
		}

        private static string HexDump(byte left)
        {
            string result = "";
            result += Digits[(left >> 4) & 0xf];
            result += Digits[left & 0xf];
            return result;
        }

		private string DumpAsBit()
		{
			string result = "";
			for(Size i= Size.Create(0); i <_size; i+=1 )
				result = "01"[GetBit(i)] + result;
			return result;
		}

        private int GetBit(Size i)
        {
            int b = i / SegmentBits;
            int p = (i % SegmentBits).ToInt();
            return (_data[b] >> p) & 1;
        }

        /// <summary/>
        public static BitsConst operator +(BitsConst left, BitsConst right){return left.Plus(right, PlusSize(left.Size,right.Size));}

	    private static Size PlusSize(Size size, Size size1)
	    {
	        return Reni.Size.Create(PlusSize(size.ToInt(), size1.ToInt()));
	    }

	    /// <summary/>
        public static BitsConst Add(BitsConst left, BitsConst right) { return left.Plus(right, PlusSize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst operator -(BitsConst left, BitsConst right) { return left.Plus(right * -1, PlusSize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst operator -(int left, BitsConst right) { return Convert(left) - right; }
        /// <summary/>
        public static BitsConst Subtract(BitsConst left, BitsConst right) { return left.Plus(right * -1, PlusSize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst Subtract(int left, BitsConst right) { return Convert(left) - right; }
        /// <summary/>
        public static BitsConst operator *(BitsConst left, BitsConst right) { return left.Multiply(right, MultiplySize(left.Size,right.Size)); }

	    private static Size MultiplySize(Size left, Size right)
	    {
            return Reni.Size.Create(MultiplySize(left.ToInt(), right.ToInt()));
        }

        private static Size DivideSize(Size left, Size right)
        {
            return Reni.Size.Create(DivideSize(left.ToInt(), right.ToInt()));
        }

        /// <summary/>
        public static BitsConst operator *(BitsConst left, int right) { return left * Convert(right); }
        /// <summary/>
        public static BitsConst Multiply(BitsConst left, BitsConst right) { return left.Multiply(right, MultiplySize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst Multiply(BitsConst left, int right) { return left * Convert(right); }
        /// <summary/>
        public static BitsConst operator /(BitsConst left, BitsConst right) { return left.Divide(right, DivideSize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst operator /(BitsConst left, int right) { return left / Convert(right); }
        /// <summary/>
        public static BitsConst Divide(BitsConst left, BitsConst right) { return left.Divide(right, DivideSize(left.Size, right.Size)); }
        /// <summary/>
        public static BitsConst Divide(BitsConst left, int right) { return left / Convert(right); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            BitsConst left = obj as BitsConst;
            if (left == null)
                return false;
            if (Size != left.Size)
                return false;
            return _data == left._data;
        }

        /// <summary>
        /// 
        /// </summary>                                                                              
        /// <returns></returns>
        public override int GetHashCode()
        {
             return _data.GetHashCode();
        }

	    /// <summary>
	    /// Handles print output
	    /// </summary>
	    public static OutStream OutStream
	    {
	        get { return _outStream; }
	        set { _outStream = value; }
	    }

        internal BitsConst Access(Intervall<Size> region)
        {
            return Access(region.Start, region.End - region.Start);
        }
        /// <summary>
        /// Extract a part of the object
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public BitsConst Access(Size start, Size size)
        {
            NotImplementedMethod(start,size);
            return this;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="index">The index.</param>
        /// [created 04.09.2006 22:43]
	    public void CopyTo(byte[] data, int index)
	    {
	        _data.CopyTo(data,index);
	    }

        /// <summary>
        /// Converts the specified value into a bits const
        /// </summary>
        /// <param name="value">The bytes.</param>
        /// <returns></returns>
        /// [created 04.09.2006 23:04]
	    public static BitsConst Convert(byte[] value)
	    {
	        return new BitsConst(value.Length,value,0);
	        
	    }

        private void AddAndKeepSize(BitsConst left)
        {
            Int16 carry = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                carry += _data[i];
                carry += left._data[i];
                _data[i] = (byte)carry;
                carry /= (Int16)SegmentValues;
            }
            return;
        }

        /// <summary>
        /// Codes the dump.
        /// </summary>
        /// <returns></returns>
        /// created 06.09.2006 23:57
	    public string CodeDump()
	    {
            return ToInt64().ToString();
	    }

	    /// <summary>
	    /// NUnit test class of BitsConst
	    /// </summary>
        [TestFixture]
        public class Test
        {
            /// <summary>
            /// Tests the bits const.
            /// </summary>
            /// created 08.10.2006 16:33
            [Test, Category(CompilerTest.Worked)]
            public void All()
            {
                Tracer.FlaggedLine("Position of method tested");
                Tracer.Assert(Convert(0).ToInt64() == 0);
                Tracer.Assert(Convert(11).ToInt64() == 11);
                Tracer.Assert(Convert(-111).ToInt64() == -111);

                Tracer.Assert((Convert(10) + Convert(1)).ToInt64() == 11);
                Tracer.Assert((Convert(10) + Convert(-1)).ToInt64() == 9);
                Tracer.Assert((Convert(1) + Convert(-10)).ToInt64() == -9);

                Tracer.Assert((Convert(1111) + Convert(-10)).ToInt64() == 1101);
                Tracer.Assert((Convert(1111) + Convert(-1110)).ToInt64() == 1);
                Tracer.Assert((Convert(1111) + Convert(-1112)).ToInt64() == -1);

                Tracer.Assert(Convert("0").ToString(10) == "0", Convert("0").ToString(10));
                Tracer.Assert(Convert("1").ToString(10) == "1", Convert("1").ToString(10));
                Tracer.Assert(Convert("21").ToString(10) == "21", Convert("21").ToString(10));
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
            /// <summary>
            /// Resizes this instance.
            /// </summary>
            /// created 17.01.2007 23:51
            [Test, Category(CompilerTest.Worked)]
            public void Resize()
            {
                Tracer.FlaggedLine("Position of method tested");
                Tracer.Assert(Convert("100").Resize(Size.Create(6)).ToString(10) == "-28");

                Tracer.Assert(Convert("1").Resize(Size.Create(1)).ToString(10) == "-1");
                Tracer.Assert(Convert("1").Resize(Size.Create(2)).ToString(10) == "1");
                Tracer.Assert(Convert("1").Resize(Size.Create(3)).ToString(10) == "1");

                Tracer.Assert(Convert("2").Resize(Size.Create(1)).ToString(10) == "0");
                Tracer.Assert(Convert("2").Resize(Size.Create(2)).ToString(10) == "-2", Convert("2").Resize(Size.Create(2)).ToString(10));
                Tracer.Assert(Convert("2").Resize(Size.Create(3)).ToString(10) == "2");

                Tracer.Assert(Convert("-4").Resize(Size.Create(32)).ToString(10) == "-4", Convert("-4").Resize(Size.Create(32)).ToString(10));
                Tracer.Assert(Convert("-4095").Resize(Size.Create(8)).ToString(10) == "1", Convert("-4095").Resize(Size.Create(32)).ToString(10));
                
            }
        }

	}
}