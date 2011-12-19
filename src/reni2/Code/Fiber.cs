//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

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
            StopByObjectId(-14);
        }

        void AssertValid()
        {
            Tracer.Assert(!_fiberHead.IsNonFiberHeadList);
            Tracer.Assert(_fiberItems.Length > 0);
            var lastSize = _fiberHead.Size;
            foreach(var t in _fiberItems)
            {
                Tracer.Assert(lastSize == t.InputSize);
                lastSize = t.OutputSize;
            }
        }

        internal Fiber(FiberHead fiberHead, FiberItem fiberItem)
            : this(fiberHead, null, fiberItem) { }

        [Node]
        internal FiberHead FiberHead { get { return _fiberHead; } }
        [Node]
        internal FiberItem[] FiberItems { get { return _fiberItems; } }
        internal override bool IsRelativeReference { get { return _fiberHead.IsRelativeReference; } }
        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _fiberItems[_fiberItems.Length - 1].RefAlignParam; } }

        protected override Size GetTemporarySize()
        {
            var result = _fiberHead.TemporarySize;
            var sizeSoFar = _fiberHead.Size;
            foreach (var codeBase in _fiberItems)
            {
                sizeSoFar -= codeBase.InputSize;
                var newResult = sizeSoFar + codeBase.TemporarySize;
                sizeSoFar += codeBase.OutputSize;
                result = result.Max(newResult).Max(sizeSoFar);
            }
            return result;
        }

        protected override Size GetSize() { return _fiberItems[_fiberItems.Length - 1].OutputSize; }

        protected override CodeArgs GetRefsImplementation()
        {
            return _fiberItems
                .Aggregate(FiberHead.CodeArgs, (current, fiberItem) => current.Sequence(fiberItem.CodeArgs));
        }

        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            var lastFiberItems = new List<FiberItem> {subsequentElement};
            var fiberItems = new List<FiberItem>(_fiberItems);
            while(lastFiberItems.Count > 0)
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
            if(fiberItems.Count <= 0)
                return _fiberHead;
            return new Fiber(_fiberHead, fiberItems, null);
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Fiber(this); }

        [DisableDump]
        internal new bool HasArg
        {
            get
            {
                if(FiberHead.HasArg)
                    return true;
                return FiberItems.Any(x => x.HasArg);
            }
        }

        internal override void Visit(IVisitor visitor) { visitor.Fiber(_fiberHead, _fiberItems); }

        public override string DumpData()
        {
            var result = "";
            result += "[*] " + _fiberHead.Dump() + "\n";
            result += _fiberItems.DumpLines();
            return result.Substring(0, result.Length - 1);
        }

        internal CodeBase ReCreate(CodeBase newHead, FiberItem[] newItems)
        {
            return (newHead ?? FiberHead)
                .CreateFiber(newItems.Select((x, i) => x ?? FiberItems[i]));
        }
    }
}