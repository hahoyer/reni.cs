using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    internal sealed class Fiber : CodeBase
    {
        private readonly FiberHead _fiberHead;
        private readonly FiberItem[] _fiberItems;

        private Fiber(FiberHead fiberHead, IEnumerable<FiberItem> fiberItems, FiberItem fiberItem = null)
        {
            _fiberHead = fiberHead;
            var l = new List<FiberItem>();
            if (fiberItems != null)
                l.AddRange(fiberItems);
            if (fiberItem != null)
                l.Add(fiberItem);
            _fiberItems = l.ToArray();
            Tracer.Assert(_fiberItems.Length > 0);
            Size lastSize = _fiberHead.Size;
            foreach(var t in _fiberItems)
            {
                Tracer.Assert(lastSize == t.InputSize);
                lastSize = t.OutputSize;
            }
        }

        internal Fiber(FiberHead fiberHead, FiberItem fiberItem)
            : this(fiberHead, null, fiberItem) { }

        internal FiberHead FiberHead { get { return _fiberHead; } }
        internal FiberItem[] FiberItems { get { return _fiberItems; } }

        protected override Size GetSize() { return _fiberItems[_fiberItems.Length - 1].OutputSize; }

        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            var lastFiberItems = new List<FiberItem> {subsequentElement};
            var fiberItems = new List<FiberItem> (_fiberItems);
            while (lastFiberItems.Count > 0)
            {
                if(fiberItems.Count > 0)
                {
                    var newLastFiberItems = fiberItems[fiberItems.Count - 1].TryToCombine(lastFiberItems[0]);
                    if(newLastFiberItems == null)
                    {
                        fiberItems.AddRange(lastFiberItems);
                        lastFiberItems.RemoveAll(x => true);
                    }
                    else
                    {
                        fiberItems.RemoveAt(fiberItems.Count - 1);
                        fiberItems.AddRange(newLastFiberItems);
                        lastFiberItems.RemoveAt(0);
                    }
                }
                else
                {
                    fiberItems.AddRange(lastFiberItems);
                    lastFiberItems.RemoveAll(x => true);
                }
            }
            return new Fiber(_fiberHead, fiberItems);
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            return actual.Fiber(this);
        }

        public override string DumpData()
        {
            var result = "";
            result += "[*] " + _fiberHead.Dump() + "\n";
            result += _fiberItems.DumpLines();
            return result.Substring(0,result.Length-1);
        }
    }

    internal abstract class FiberItem : ReniObject
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

        protected abstract string Format(StorageDescriptor start);
        internal abstract void Execute(IFormalMaschine formalMaschine);

        internal virtual FiberItem[] TryToCombine(FiberItem subsequentElement) { return null; }

        internal virtual CodeBase TryToCombineBack(BitArray precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(FrameRef precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopData precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopFrame precedingElement) { return null; }
        internal virtual CodeBase TryToCombineBack(TopRef precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(BitCast precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(Dereference precedingElement) { return null; }
        internal virtual FiberItem[] TryToCombineBack(RefPlus precedingElement) { return null; }

    
        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            NotImplementedMethod(actual);
            return default(TResult);
        }
    }

    internal abstract class FiberHead : CodeBase
    {
        protected FiberHead(int objectId)
            : base(objectId) { }

        protected FiberHead() { }

        protected virtual string Format(StorageDescriptor start)
        {
            NotImplementedMethod(start);
            return "";
        }

        internal virtual void Execute(IFormalMaschine formalMaschine) { NotImplementedMethod(formalMaschine); }

        protected virtual CodeBase TryToCombine(FiberItem subsequentElement) { return null; }
        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            return TryToCombine(subsequentElement) ?? new Fiber(this, subsequentElement);
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return default(TResult); }
    }
}