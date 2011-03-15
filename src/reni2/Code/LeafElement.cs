using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Obsolete]
    internal abstract class LeafElement : ReniObject, IIconKeyProvider
    {
        private static int _nextId;

        protected LeafElement(int objectId)
            : base(objectId) { }

        protected LeafElement()
            : base(_nextId++) { }

        [Node, IsDumpEnabled(false)]
        internal Size DeltaSize { get { return GetInputSize() - GetSize(); } }

        [IsDumpEnabled(false)]
        internal virtual bool IsEmpty { get { return false; } }

        [Node, IsDumpEnabled(false)]
        internal Size Size { get { return GetSize(); } }

        private string CommentDump { get { return GetType().Name + " " + ObjectId; } }

        [IsDumpEnabled(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        protected abstract Size GetSize();
        protected abstract Size GetInputSize();

        internal string Statements(CSharpGenerator start)
        {
            var result = Format(start);
            if(result != "")
                result += "; // " + CommentDump + "\n";
            return result;
        }

        protected virtual string Format(CSharpGenerator start)
        {
            NotImplementedMethod(start);
            throw new NotImplementedException();
        }

        internal virtual LeafElement[] TryToCombineBackN(TopData precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if(result == null)
                return null;
            return new[] {result};
        }

        internal virtual LeafElement[] TryToCombineBackN(BitArrayBinaryOp precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if(result == null)
                return null;
            return new[] {result};
        }

        internal virtual LeafElement[] TryToCombineBackN(BitArrayPrefixOp precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if(result == null)
                return null;
            return new[] {result};
        }

        internal virtual LeafElement[] TryToCombineBackN(Dereference precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if(result == null)
                return null;
            return new[] {result};
        }

        internal virtual LeafElement[] TryToCombineBackN(BitCast precedingElement)
        {
            var result = TryToCombineBack(precedingElement);
            if(result == null)
                return null;
            return new[] {result};
        }

        internal virtual LeafElement TryToCombineBack(Dereference precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(TopRef precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(BitCast precedingElement) { return null; }

        internal virtual FiberHead TryToCombineBack(TopFrameRef precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(BitArray precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(TopData precedingElement) { return null; }

        internal virtual FiberHead TryToCombineBack(TopFrameData precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(BitArrayBinaryOp precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(BitArrayPrefixOp precedingElement) { return null; }

        internal virtual LeafElement TryToCombineBack(RefPlus precedingElement) { return null; }

        internal virtual LeafElement Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity) { return this; }

        internal virtual BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return IsError ? "CodeError" : "Code"; } }

        [DumpExcept(false)]
        protected virtual bool IsError { get { return false; } }

        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }

        internal virtual void Execute(IFormalMaschine formalMaschine) { NotImplementedMethod(formalMaschine); }
    }
}