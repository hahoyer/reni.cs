using System;
using HWClassLibrary.Debug;

namespace Reni.Context
{
    /// <summary>
    /// Parameter to describe alignment for references
    /// </summary>
    [dump("Dump")]
    [Serializable]
    sealed internal class RefAlignParam : IEquatable<RefAlignParam>
    {
        private readonly int _alignBits;
        private readonly Size _refSize;

        public RefAlignParam(int alignBits, Size refSize)
        {
            _alignBits = alignBits;
            _refSize = refSize;
        }

        public int AlignBits { get { return _alignBits; } }
        public Size RefSize { get { return _refSize; } }

        public bool Is_3_32 { get { return _alignBits == BitsConst.SegmentAlignBits && _refSize.ToInt() == 32; } }

        public RefAlignParam Align(int alignBits)
        {
            var newAlignBits = Math.Max(AlignBits, alignBits);
            if(newAlignBits == AlignBits)
                return this;
            return new RefAlignParam(newAlignBits, RefSize);
        }

        public static Size Offset(SizeArray list, int index)
        {
            var result = Size.Create(0);
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

        public string Dump()
        {
            return "[A:" + AlignBits + ",S:" + RefSize.Dump() + "]";
        }

        public string CodeDump()
        {
            return AlignBits + "_" + RefSize.ToInt();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_alignBits*397) ^ (_refSize != null ? _refSize.GetHashCode() : 0);
            }
        }

        public bool Equals(RefAlignParam obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj._alignBits == _alignBits && Equals(obj._refSize, _refSize);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != typeof(RefAlignParam))
                return false;
            return Equals((RefAlignParam) obj);
        }

        public static bool operator ==(RefAlignParam left, RefAlignParam right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RefAlignParam left, RefAlignParam right)
        {
            return !Equals(left, right);
        }

    }
}