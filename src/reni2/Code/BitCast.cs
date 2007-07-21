using System;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// Expression to change size of an expression
    /// </summary>
    sealed public class BitCast : LeafElement
    {
        private readonly Size _size;
        private readonly Size _significantSize;
        private readonly Size _targetSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BitCast"/> class.
        /// </summary>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="size">The size.</param>
        /// <param name="significantSize">Size of the min.</param>
        /// created 23.09.2006 18:06
        public BitCast(Size targetSize, Size size, Size significantSize)
        {
            _size = size;
            _significantSize = significantSize;
            _targetSize = targetSize;
        }
        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>
        /// null if no combination possible (default) or a leaf element that contains the combination of both
        /// </returns>
        /// created 19.10.2006 21:18
        /// created 18.11.2006 14:53
        public override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            if (Size == TargetSize && Size == SignificantSize)
                return subsequentElement;
            return subsequentElement.TryToCombineBack(this);
        }


        /// <summary>
        /// Tries to combine a leaf element with a preceding <see cref="BitCast"/> element.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.11.2006 19:13
        public override LeafElement TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement.Size != TargetSize)
                return null;
            
            return new BitCast
                (
                precedingElement.TargetSize,
                Size, 
                _significantSize.Min(precedingElement.SignificantSize)
                );
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 03:50
        public override LeafElement TryToCombineBack(BitArray precedingElement)
        {
            BitsConst bitsConst = precedingElement.Data;
            if (bitsConst.Size > SignificantSize)
                bitsConst = bitsConst.Resize(SignificantSize);
            return new BitArray(Size, bitsConst);
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:07
        public override LeafElement TryToCombineBack(TopData precedingElement)
        {
            return new TopData(precedingElement.RefAlignParam,precedingElement.Offset, precedingElement.TargetSize, Size);
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:07
        public override LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            return new TopFrame(precedingElement.RefAlignParam, precedingElement.Offset, precedingElement.TargetSize, Size);
        }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:57
        public override LeafElement TryToCombineBack(BitArrayOp precedingElement)
        {
            return new BitArrayOp(precedingElement.OpToken,Size, precedingElement.LeftSize,precedingElement.RightSize);
        }

        /// <summary>
        /// Tries to combine a leaf element with a preceding <see cref="Dereference"/> element.
        /// </summary>
        /// <param name="precedingElement">the preceding element.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:25
        public override LeafElement TryToCombineBack(Dereference precedingElement)
        {
            Tracer.Assert(TargetSize == precedingElement.Size);
            return new Dereference(precedingElement.RefAlignParam,precedingElement.TargetSize,Size);
        }
      
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        /// created 23.09.2006 18:06
        public override Size Size { get { return _size; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return TargetSize - Size; } }

        /// <summary>
        /// Significant size 
        /// </summary>
        public Size SignificantSize { get { return _significantSize; } }
        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.BitCast(TargetSize,Size,SignificantSize);
        }

        /// <summary>
        /// Gets the size of the target.
        /// </summary>
        /// <value>The size of the target.</value>
        /// created 05.10.2006 23:24
        public Size TargetSize { get { return _targetSize; } }

    }
}