#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.TreeStructure;
using Reni.Basics;
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

            StopByObjectId(-61);
        }

        void AssertValid()
        {
            Tracer.Assert(!_fiberHead.IsNonFiberHeadList, Dump);
            Tracer.Assert(_fiberItems.Length > 0, Dump);
            var lastSize = _fiberHead.Size;
            foreach(var t in _fiberItems)
            {
                Tracer.Assert(lastSize == t.InputSize, Dump);
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

        protected override Size GetSize() { return _fiberItems[_fiberItems.Length - 1].OutputSize; }

        protected override CodeArgs GetRefsImplementation()
        {
            return _fiberItems
                .Aggregate(FiberHead.CodeArgs, (current, fiberItem) => current.Sequence(fiberItem.CodeArgs));
        }

        internal override CodeBase Add(FiberItem subsequentElement)
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
        internal override IEnumerable<IssueBase> Issues
        {
            get
            {
                return _fiberHead
                    .Issues
                    .Union(_fiberItems.SelectMany(item => item.Issues));
            }
        }

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
                .AddRange(newItems.Select((x, i) => x ?? FiberItems[i]));
        }
    }
}