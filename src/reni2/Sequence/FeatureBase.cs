using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class FeatureBase : ReniObject, ISearchPath<IFeature, SequenceType>
    {
        [EnableDump]
        protected internal readonly ISequenceOfBitBinaryOperation Definable;

        protected FeatureBase(ISequenceOfBitBinaryOperation definable) { Definable = definable; }

        IFeature ISearchPath<IFeature, SequenceType>.Convert(SequenceType type) { return type.Feature(this); }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }
}