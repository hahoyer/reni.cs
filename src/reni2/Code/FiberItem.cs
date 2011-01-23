using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal abstract class FiberItem : ReniObject, IFormalCodeItem
    {
        protected FiberItem(int objectId)
            : base(objectId) { }

        protected FiberItem() { }

        [IsDumpEnabled(false)]
        internal abstract Size InputSize { get; }

        [IsDumpEnabled(false)]
        internal abstract Size OutputSize { get; }

        [IsDumpEnabled(false)]
        internal Size DeltaSize { get { return OutputSize - InputSize; } }

        internal string ReversePolish(Size top)
        {
            return CSharpCodeSnippet(top) + ";\n";
        }

        protected virtual string CSharpCodeSnippet(Size top)
        {
            NotImplementedMethod(top);
            return null;
        }


        internal virtual FiberItem[] TryToCombine(FiberItem subsequentElement) { return null; }

        internal virtual CodeBase TryToCombineBack(BitArray precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopFrameRef precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopData precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopFrameData precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopRef precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(LocalVariableReference precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitCast preceding) { return null; }
        internal virtual FiberItem[] TryToCombineBack(Dereference preceding) { return null; }
        internal virtual FiberItem[] TryToCombineBack(RefPlus precedingElement) { return null; }


        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            NotImplementedMethod(actual);
            return default(TResult);
        }

        protected abstract void Execute(IFormalMaschine formalMaschine);

        void IFormalCodeItem.Execute(IFormalMaschine formalMaschine)
        {
            Execute(formalMaschine);
        }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + DumpSignature; } }

        [IsDumpEnabled(false)]
        private string DumpSignature { get { return "(" + InputSize + "==>" + OutputSize + ")"; } }

        [IsDumpEnabled(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        internal Refs Refs { get { return GetRefsImplementation(); } }
        protected virtual Refs GetRefsImplementation() { return Refs.None(); }
    }

    internal interface IFormalCodeItem
    {
        void Execute(IFormalMaschine formalMaschine);
        string Dump();
    }
}