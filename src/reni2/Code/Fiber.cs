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

        private Fiber(FiberHead fiberHead, IEnumerable<FiberItem> fiberItems, FiberItem fiberItem)
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

        internal Fiber(FiberHead fiberHead)
            : this(fiberHead, null, null) { }

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
            if(fiberItems.Count <= 0)
                return _fiberHead;
            return new Fiber(_fiberHead, fiberItems, null);
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            return actual.Fiber(this);
        }

        internal override CSharpCodeSnippet CSharpCodeSnippet() { return CSharpGenerator.Fiber(_fiberItems, _fiberHead); }

        public override string DumpData()
        {
            var result = "";
            result += "[*] " + _fiberHead.Dump() + "\n";
            result += _fiberItems.DumpLines();
            return result.Substring(0,result.Length-1);
        }

        internal CodeBase List() { return Code.List.Create(this); }
    }
}