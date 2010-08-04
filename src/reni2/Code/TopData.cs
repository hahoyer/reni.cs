using System;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Combination of TopRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopData : Top
    {
        public TopData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize)
        {
            StopByObjectId(-110);
        }
        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopData(Offset, Size, DataSize); }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.CreateTopData(RefAlignParam, Offset, GetSize(), DataSize);
        }
    }

    /// <summary>
    /// Combination of FrameRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopFrame : Top
    {
        public TopFrame(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize)
        {
            StopByObjectId(544);
        }
        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopFrame(Offset, Size, DataSize); }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.CreateTopFrame(RefAlignParam, Offset, GetSize(), DataSize);
        }

    }
}