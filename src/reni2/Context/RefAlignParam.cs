using System;
using HWClassLibrary.Debug;

namespace Reni.Context
{
    /// <summary>
    /// Parameter to describe alignment for references
    /// </summary>
   [dump("Dump")]
    internal class RefAlignParam
    {
        private readonly int _alignBits;
        private readonly Size _refSize;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="alignBits"></param>
        /// <param name="refSize"></param>
        public RefAlignParam(int alignBits, Size refSize)
        {
            _alignBits = alignBits;
            _refSize = refSize;
        }
        /// <summary>
        /// asis
        /// </summary>
        public int AlignBits{get { return _alignBits; }}
        /// <summary>
        /// asis
        /// </summary>
        public Size RefSize { get { return _refSize; } }

        /// <summary>
        /// Checks if width of ref is 32 bit and precision is 3 bit (= byte)
        /// </summary>
        /// <value><c>true</c> if is_3_32; otherwise, <c>false</c>.</value>
        /// created 10.10.2006 22:48
        public bool Is_3_32 { get { return _alignBits == 3 && _refSize.ToInt() == 32; } }

        /// <summary>
        /// align to alignbits, size remains
        /// </summary>
        /// <param name="alignBits"></param>
        /// <returns></returns>
        public RefAlignParam Align(int alignBits)
        {
            int newAlignBits = Math.Max(AlignBits, alignBits);
            if(newAlignBits == AlignBits)
                return this;
            return new RefAlignParam(newAlignBits, RefSize);
        }

        /// <summary>
        /// Calculate aligned offset of type element
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Size Offset(SizeArray list, int index)
        {
            Size result = Size.Create(0);
            for (int i = index + 1; i < list.Count; i++)
                result += list[i];
            return result;
        }

        /// <summary>
        /// Determines whether the specified param is equal.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>
        /// 	<c>true</c> if the specified param is equal; otherwise, <c>false</c>.
        /// </returns>
        /// [created 18.05.2006 22:28]
        public bool IsEqual(RefAlignParam param)
        {
            if(param.AlignBits != AlignBits)
                return false;

            if (param.RefSize != RefSize)
                return false;
            
            return true;
        }
        /// <summary>
        /// Dumps this instance.
        /// </summary>
        /// <returns></returns>
        /// [created 06.07.2006 22:53]
        public string Dump()
        {
            return "[A:" + AlignBits.ToString() + ",S:" + RefSize.Dump() + "]";
        }

        /// <summary>
        /// Codes the dump.
        /// </summary>
        /// <returns></returns>
        /// created 06.09.2006 23:58
        public string CodeDump()
        {
            return AlignBits.ToString() + "_" + RefSize.ToInt();
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// created 19.10.2006 21:54
        public override int GetHashCode()
        {
            return _alignBits + 29*_refSize.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        /// created 19.10.2006 21:54
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            RefAlignParam refAlignParam = obj as RefAlignParam;
            if (_alignBits != refAlignParam._alignBits) return false;
            if (!Equals(_refSize, refAlignParam._refSize)) return false;
            return true;
        }
    }
}
