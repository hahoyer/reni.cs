using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference to something 
    /// </summary>
    internal abstract class RefCode : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        [Node]
        internal readonly Size Offset;

        protected RefCode(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize()
        {
            return _refAlignParam.RefSize;
        }

        protected override Size GetDeltaSize()
        {
            return GetSize()*-1;
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}