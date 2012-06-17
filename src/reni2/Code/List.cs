using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    internal sealed class List : FiberHead
    {
        private readonly CodeBase[] _data;
        private static int _nextObjectId;

        [Node]
        internal CodeBase[] Data { get { return _data; } }

        internal static List Create(params CodeBase[] data) { return new List(data); }
        internal static List Create(IEnumerable<CodeBase> data) { return new List(data); }

        private void AssertValid()
        {
            foreach(var codeBase in _data)
            {
                Tracer.Assert(!(codeBase is List));
                Tracer.Assert(!(codeBase.IsEmpty));
            }
            Tracer.Assert(_data.Length > 1);
        }

        private List(IEnumerable<CodeBase> data)
            : base(_nextObjectId++)
        {
            _data = data.ToArray();
            AssertValid();
            StopByObjectId(-10);
        }

        protected override IEnumerable<CodeBase> AsList() { return _data; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.List(this); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            if(IsNonFiberHeadList)
            {
                var newData = new CodeBase[_data.Length];
                var i = 0;
                for(; i < _data.Length - 1; i++)
                    newData[i] = _data[i];
                newData[i] = _data[i].CreateFiber(subsequentElement);
                return List(newData);
            }
            return null;
        }

        [DisableDump]
        internal override bool IsNonFiberHeadList
        {
            get
            {
                for(var i = 0; i < _data.Length - 1; i++)
                    if(!_data[i].Size.IsZero)
                        return false;
                return true;
            }
        }

        protected override Size GetTemporarySize()
        {
            var result = Size.Zero;
            var sizeSoFar = Size.Zero;
            foreach(var codeBase in _data)
            {
                var newResult = sizeSoFar + codeBase.TemporarySize;
                sizeSoFar += codeBase.Size;
                result = result.Max(newResult).Max(sizeSoFar);
            }
            return result;
        }

        protected override Size GetSize()
        {
            return _data
                .Aggregate(Size.Zero, (size, codeBase) => size + codeBase.Size);
        }

        protected override CodeArgs GetRefsImplementation() { return GetRefs(_data); }
        internal override void Visit(IVisitor visitor) { visitor.List(_data); }
    }
}