using System.Collections;
using System.Diagnostics;
using hw.UnitTest;

namespace Reni.Basics;

[AdditionalNodeInfo("DebuggerDumpString")]
[DebuggerDisplay("{NodeDump,nq}")]
public sealed class Size
    : DumpableObject
        , IIconKeyProvider
        , IComparable<Size>
        , IAggregateable<Size>
{
    [UnitTest]
    public sealed class Tests
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
            (xs.GetNextPacketSize(BitsConst.SegmentAlignBits) == Create(b)).Assert();
        }
    }

    static readonly Hashtable Values = new();
    static int NextObjectId;
    readonly int Value;

    public bool IsZero => Value == 0;

    public int SaveByteCount => GetSaveSizeToPacketCount(BitsConst.SegmentAlignBits);
    public static Size Zero => Create(0);
    public static Size Bit => Create(1);
    public static Size Byte => Bit.ByteAlignedSize;
    public bool IsPositive => Value > 0;
    public int ByteCount => GetPacketCount(BitsConst.SegmentAlignBits);
    public Size ByteAlignedSize => GetNextPacketSize(BitsConst.SegmentAlignBits);
    public static Size TextItemSize => Create(8);

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

    Size(int value)
        : base(NextObjectId++)
        => Value = value;

    Size IAggregateable<Size>.Aggregate(Size? other) => this + other!;

    int IComparable<Size>.CompareTo(Size? other) => LessThan(other!)? -1 :
        other!.LessThan(this)? 1 : 0;


    /// <summary>
    ///     Gets the icon key.
    /// </summary>
    /// <value>The icon key.</value>
    string IIconKeyProvider.IconKey => "Size";

    public override string ToString() => ToInt().ToString();

    protected override string Dump(bool isRecursion) => GetNodeDump();

    protected override string GetNodeDump() => Value.ToString();

    public static Size Create(int x)
    {
        var result = (Size?)Values[x];
        if(result == null)
        {
            result = new(x);
            Values[x] = result;
        }

        return result;
    }

    public Size GetAlign(int alignBits)
    {
        var result = GetPacketCount(alignBits) << alignBits;
        if(result == Value)
            return this;
        return Create(result);
    }

    public int GetPacketCount(int alignBits) => ((Value - 1) >> alignBits) + 1;

    public Size GetNextPacketSize(int alignBits)
        => Create(GetPacketCount(alignBits) << alignBits);

    public int ToInt() => Value;

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

    public Size GetMax(Size x)
    {
        if(Value > x.Value)
            return this;
        return x;
    }

    public Size GetMin(Size x)
    {
        if(Value < x.Value)
            return this;
        return x;
    }

    public string ToCCodeByteType() => "byte<" + ByteCount + ">";

    [UsedImplicitly]
    public string CodeDump() => ByteCount.ToString();

    internal static Size GetAutoSize(long value)
    {
        var size = 1;
        var xn = value >= 0? value : -value;
        for(long upper = 1; xn >= upper; size++, upper *= 2) { }

        return Create(size);
    }

    internal void AssertAlignedSize(int alignBits)
    {
        var result = GetPacketCount(alignBits);
        if(result << alignBits == Value)
            return;
        NotImplementedMethod(alignBits);

        throw new NotAlignableException(this, alignBits);
    }

    internal string FormatForView() => ToString() + " " + ToCCodeByteType();

    int GetSaveSizeToPacketCount(int alignBits)
    {
        AssertAlignedSize(alignBits);
        return GetPacketCount(alignBits);
    }

    bool LessThan(Size x) => Value < x.Value;

    Size Modulo(Size x) => Create(Value % x.Value);

    Size Plus(int y) => Create(Value + y);

    Size Times(int y) => Create(Value * y);

    Size Minus(int y) => Create(Value - y);

    Size Divide(int y) => Create(Value / y);

    Size Plus(Size y) => Create(Value + y.Value);

    int Divide(Size y) => Value / y.Value;

    Size Minus(Size y) => Create(Value - y.Value);
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