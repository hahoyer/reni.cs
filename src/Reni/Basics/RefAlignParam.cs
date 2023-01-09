using System.Diagnostics;
using hw.DebugFormatter;

namespace Reni.Basics;

[Dump("Dump")]
[DebuggerDisplay("{CodeDump,nq}")]
sealed class RefAlignParam : IEquatable<RefAlignParam>
{
    public int AlignBits { get; }

    public Size RefSize { get; }

    internal string CodeDump => AlignBits + "/" + RefSize.ToInt();

    public RefAlignParam(int alignBits, Size refSize)
    {
        AlignBits = alignBits;
        RefSize = refSize;
    }

    public bool Equals(RefAlignParam obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        return obj.AlignBits == AlignBits && Equals(obj.RefSize, RefSize);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (AlignBits * 397) ^ (RefSize != null? RefSize.GetHashCode() : 0);
        }
    }

    public override bool Equals(object obj)
    {
        if(ReferenceEquals(null, obj))
            return false;
        if(ReferenceEquals(this, obj))
            return true;
        if(obj.GetType() != typeof(RefAlignParam))
            return false;
        return Equals((RefAlignParam)obj);
    }

    public RefAlignParam Align(int alignBits)
    {
        var newAlignBits = Math.Max(AlignBits, alignBits);
        if(newAlignBits == AlignBits)
            return this;
        return new(newAlignBits, RefSize);
    }

    public static Size Offset(SizeArray list, int index)
    {
        var result = Size.Zero;
        for(var i = index + 1; i < list.Count; i++)
            result += list[i];
        return result;
    }

    public bool IsEqual(RefAlignParam param)
    {
        if(param.AlignBits != AlignBits)
            return false;

        if(param.RefSize != RefSize)
            return false;

        return true;
    }

    public string Dump() => "[A:" + AlignBits + ",S:" + RefSize.Dump() + "]";

    public static bool operator ==(RefAlignParam left, RefAlignParam right) => Equals(left, right);

    public static bool operator !=(RefAlignParam left, RefAlignParam right) => !Equals(left, right);
}