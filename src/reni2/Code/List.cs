﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;
using Reni.Validation;

namespace Reni.Code
{
    sealed class List : FiberHead
    {
        static int _nextObjectId;

        [Node]
        internal CodeBase[] Data { get; }

        internal static CodeBase Create(params CodeBase[] data)
        {
            var d = data.Where(item => item != null).ToArray();
            switch(d.Length)
            {
                case 0:
                    return Void;
                case 1:
                    return d.First();
                default:
                    return new List(d);
            }
        }

        internal static CodeBase CheckedCreate(IEnumerable<CodeBase> data)
        {
            if(data == null)
                return null;

            var dataArray = data.ToArray();

            Tracer.Assert(!dataArray.Any(item => item is IssueCode));

            if(!dataArray.Any())
                return null;
            if(dataArray.Length == 1)
                return dataArray[0];
            return new List(dataArray);
        }

        void AssertValid()
        {
            foreach(var codeBase in Data)
            {
                Tracer.Assert(!(codeBase is List), () => codeBase.Dump());
                Tracer.Assert(!(codeBase.IsEmpty));
            }
            Tracer.Assert(Data.Length > 1);
        }

        List(IEnumerable<CodeBase> data)
            : base(_nextObjectId++)
        {
            Data = data.ToArray();
            AssertValid();
            StopByObjectIds();
        }

        protected override IEnumerable<CodeBase> AsList() => Data;

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.List(this);

        [DisableDump]
        internal override IEnumerable<Issue> Issues => Data.SelectMany(data => data.Issues);

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

        internal TypeBase Visit(Visitor<TypeBase, TypeBase> argTypeVisitor)
            => Data
                .Select(x => x.Visit(argTypeVisitor))
                .DistinctNotNull();
    }
}