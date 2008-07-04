using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal abstract class LeafElement : ReniObject
    {
        [DumpData(false)]
        public abstract Size Size { get; }
        [DumpData(false)]
        public virtual bool IsEmpty { get { return false; } }
        [DumpData(false)]
        public abstract Size DeltaSize { get; }
        private string CommentDump { get { return GetType().Name + " " + ObjectId; } }
        [DumpData(false)]
        public virtual RefAlignParam RefAlignParam { get { return null; } }

        public string Statements(StorageDescriptor start)
        {
            var result = Format(start);
            if(result != "")
                result += "; // " + CommentDump + "\n";
            return result;
        }

        protected virtual string Format(StorageDescriptor start)
        {
            NotImplementedMethod(start);
            throw new NotImplementedException();
        }

        internal virtual LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return null;
        }

        internal virtual LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            var result = TryToCombine(subsequentElement);
            if (result == null)
                return null;
            return new[]{result};
        }

        internal virtual LeafElement TryToCombineBack(Dereference precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(TopRef precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(BitCast precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(BitArray precedingElement)
        {
            return null;
        }

        internal virtual LeafElement[] TryToCombineBack(TopData precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(BitArrayOp precedingElement)
        {
            return null;
        }

        internal virtual LeafElement TryToCombineBack(RefPlus precedingElement)
        {
            return null;
        }

        internal virtual LeafElement Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity)
        {
            return this;
        }

        internal virtual BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }
    }
}