using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class FeatureBase : ReniObject, ISearchPath<IFeature, BaseType>
    {
        [EnableDump]
        protected internal readonly ISequenceOfBitBinaryOperation Definable;

        protected FeatureBase(ISequenceOfBitBinaryOperation definable) { Definable = definable; }

        IFeature ISearchPath<IFeature, BaseType>.Convert(BaseType type) { return type.Feature(this); }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }
}