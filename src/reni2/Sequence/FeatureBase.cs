using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class FeatureBase 
        : ReniObject
        , ISearchPath<ISuffixFeature, SequenceType>
        , ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.Array>
    {
        [EnableDump]
        internal readonly ISequenceOfBitBinaryOperation Definable;

        protected FeatureBase(ISequenceOfBitBinaryOperation definable) { Definable = definable; }

        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type) { return type.Feature(this); }
        ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.Array>.Convert(Type.Array type) { return this; }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }
}