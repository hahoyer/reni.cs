using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Validation;

namespace Reni.Code
{
    sealed class List : FiberHead
    {
        static int _nextObjectId;

        [Node]
        internal CodeBase[] Data { get; }

        internal static List Create(params CodeBase[] data) => new List(data);
        internal static List Create(IEnumerable<CodeBase> data) => new List(data);

        void AssertValid()
        {
            foreach(var codeBase in Data)
            {
                Tracer.Assert(!(codeBase is List));
                Tracer.Assert(!(codeBase.IsEmpty));
            }
            Tracer.Assert(Data.Length > 1);
        }

        List(IEnumerable<CodeBase> data)
            : base(_nextObjectId++)
        {
            Data = data.ToArray();
            AssertValid();
            StopByObjectId(-10);
        }

        protected override IEnumerable<CodeBase> AsList() => Data;
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
            => actual.List(this);
        [DisableDump]
        internal override IEnumerable<IssueBase> Issues => Data.SelectMany(data => data.Issues);

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            if(!IsNonFiberHeadList)
                return subsequentElement.TryToCombineBack(this);
            var newData = new CodeBase[Data.Length];
            var i = 0;
            for(; i < Data.Length - 1; i++)
                newData[i] = Data[i];
            newData[i] = Data[i].Add(subsequentElement);
            return List(newData);
        }

        [DisableDump]
        internal override bool IsNonFiberHeadList
        {
            get
            {
                for(var i = 0; i < Data.Length - 1; i++)
                    if(!Data[i].Hllw)
                        return false;
                return true;
            }
        }

        protected override Size GetTemporarySize()
        {
            var result = Size.Zero;
            var sizeSoFar = Size.Zero;
            foreach(var codeBase in Data)
            {
                var newResult = sizeSoFar + codeBase.TemporarySize;
                sizeSoFar += codeBase.Size;
                result = result.Max(newResult).Max(sizeSoFar);
            }
            return result;
        }

        protected override Size GetSize()
            => Data
                .Aggregate(Size.Zero, (size, codeBase) => size + codeBase.Size);

        protected override CodeArgs GetRefsImplementation() => GetRefs(Data);
        internal override void Visit(IVisitor visitor) => visitor.List(Data);

        internal bool IsCombinePossible(RecursiveCallCandidate recursiveCallCandidate)
        {
            if(!(recursiveCallCandidate.DeltaSize + Size).IsZero)
                return false;

            var topFrameDatas = Data.Select(element => element as TopFrameData).ToArray();
            if(topFrameDatas.Any(element => element == null))
                return false;

            var size = Size;
            foreach(var d in topFrameDatas)
            {
                if(d.Size + d.Offset != size)
                    return false;
                size -= d.Size;
            }

            return size.IsZero;
        }
    }
}