using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;
using Reni.Validation;

namespace Reni.Code
{
    sealed class Fiber : CodeBase
    {
        readonly FiberHead _fiberHead;
        readonly FiberItem[] _fiberItems;
        static int _nextObjectId;

        Fiber(FiberHead fiberHead, IEnumerable<FiberItem> fiberItems, FiberItem fiberItem)
            : base(_nextObjectId++)
        {
            _fiberHead = fiberHead;
            var l = new List<FiberItem>();
            if(fiberItems != null)
                l.AddRange(fiberItems);
            if(fiberItem != null)
                l.Add(fiberItem);
            _fiberItems = l.ToArray();
            AssertValid();

            StopByObjectIds(21);
        }

        void AssertValid()
        {
            Tracer.Assert(!_fiberHead.IsNonFiberHeadList, Dump);
            Tracer.Assert(_fiberItems.Any(), Dump);
            var lastSize = _fiberHead.Size;
            foreach(var t in _fiberItems)
            {
                Tracer.Assert(lastSize == t.InputSize, Dump);
                lastSize = t.OutputSize;
            }
        }

        internal Fiber(FiberHead fiberHead, FiberItem fiberItem)
            : this(fiberHead, null, fiberItem) {}

        [Node]
        internal FiberHead FiberHead => _fiberHead;
        [Node]
        internal FiberItem[] FiberItems => _fiberItems;
        internal override bool IsRelativeReference => _fiberHead.IsRelativeReference;

        protected override Size GetTemporarySize()
        {
            var result = _fiberHead.TemporarySize;
            var sizeSoFar = _fiberHead.Size;
            foreach(var codeBase in _fiberItems)
            {
                sizeSoFar -= codeBase.InputSize;
                var newResult = sizeSoFar + codeBase.TemporarySize;
                sizeSoFar += codeBase.OutputSize;
                result = result.Max(newResult).Max(sizeSoFar);
            }
            return result;
        }

        protected override Size GetSize() => _fiberItems.Last().OutputSize;

        protected override CodeArgs GetRefsImplementation()
            => _fiberItems
                .Aggregate
                (FiberHead.Exts, (current, fiberItem) => current.Sequence(fiberItem.CodeArgs));

        internal override CodeBase Add(FiberItem subsequentElement)
        {
            var lastFiberItems = new List<FiberItem>
            {
                subsequentElement
            };
            var fiberItems = new List<FiberItem>(_fiberItems);
            while(lastFiberItems.Count > 0)
                if(fiberItems.Count > 0)
                {
                    var newLastFiberItems = fiberItems.Last().TryToCombine(lastFiberItems[0]);
                    if(newLastFiberItems == null)
                    {
                        fiberItems.AddRange(lastFiberItems);
                        lastFiberItems.RemoveAll(x => true);
                    }
                    else
                    {
                        fiberItems.Remove(fiberItems.Last());
                        fiberItems.AddRange(newLastFiberItems);
                        lastFiberItems.RemoveAt(0);
                    }
                }
                else
                {
                    fiberItems.AddRange(lastFiberItems);
                    lastFiberItems.RemoveAll(x => true);
                }
            if(fiberItems.Count <= 0)
                return _fiberHead;
            return new Fiber(_fiberHead, fiberItems, null);
        }

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.Fiber(this);

        internal override IEnumerable<Issue> Issues
            => _fiberHead
                .Issues
                .Union(_fiberItems.SelectMany(item => item.Issues));

        [DisableDump]
        internal new bool HasArg => FiberHead.HasArg || FiberItems.Any(x => x.HasArg);

        internal override void Visit(IVisitor visitor)
            => visitor.Fiber(_fiberHead, _fiberItems);

        public override string DumpData()
        {
            var result = "";
            result += "[*] " + _fiberHead.Dump() + "\n";
            result += _fiberItems.DumpLines();
            return result.Substring(0, result.Length - 1);
        }

        internal CodeBase ReCreate(CodeBase newHead, FiberItem[] newItems)
            => (newHead ?? FiberHead)
                .AddRange(newItems.Select((x, i) => x ?? FiberItems[i]));

        internal TypeBase Visit(Visitor<TypeBase, TypeBase> argTypeVisitor)
            => new[] {FiberHead.Visit(argTypeVisitor)}
                .Concat(FiberItems.Select(x => x.Visit(argTypeVisitor)))
                .DistinctNotNull();
    }
}