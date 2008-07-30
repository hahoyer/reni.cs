using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    internal abstract class LeafElement : ReniObject, IIconKeyProvider
    {
        [Node, DumpData(false)]
        internal Size DeltaSize { get { return GetDeltaSize(); } }
        [DumpData(false)]
        internal virtual bool IsEmpty { get { return false; } }
        [Node]
        internal Size Size { get { return GetSize(); } }
        private string CommentDump { get { return GetType().Name + " " + ObjectId; } }
        [DumpData(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        protected abstract Size GetSize();
        protected abstract Size GetDeltaSize();

        internal string Statements(StorageDescriptor start)
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

        internal virtual LeafElement[] TryToCombineBackN(TopData precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if (result == null)
                return null;
            return new[] { result };
        }

        internal virtual LeafElement[] TryToCombineBackN(TopFrame precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if (result == null)
                return null;
            return new[] { result };
        }


        internal virtual LeafElement[] TryToCombineBackN(BitArrayOp precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if (result == null)
                return null;
            return new[] { result };
        }

        internal virtual LeafElement[] TryToCombineBackN(Dereference precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if (result == null)
                return null;
            return new[] { result };
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

        internal virtual LeafElement TryToCombineBack(TopData precedingElement)
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

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return IsError? "CodeError": "Code"; } }
        [DumpExcept(false)]
        protected virtual bool IsError { get { return false; } }
        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }
    }
}