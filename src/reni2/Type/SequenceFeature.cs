using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;

namespace Reni.Type
{
    internal abstract class SequenceFeatureBase : ReniObject, ISearchPath<IFeature, Sequence>
    {
        [DumpData(true)]
        protected internal readonly ISequenceOfBitBinaryOperation Definable;

        protected SequenceFeatureBase(ISequenceOfBitBinaryOperation definable)
        {
            Definable = definable;
        }

        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return type.Feature(this); }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }

    internal class SequenceFeature : SequenceFeatureBase

    {
        public SequenceFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable) {
            }

        internal override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.CreateNumber(Definable.ResultSize(objSize, argsSize));
        }
    }
    internal class SequenceCompareFeature : SequenceFeatureBase
    {
        public SequenceCompareFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable)
        {
        }

        internal override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.CreateBit;
        }
    }
}