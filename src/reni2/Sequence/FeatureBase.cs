using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class FeatureBase 
        : ReniObject
        , ISearchPath<ISuffixFeature, SequenceType>
        , ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.ArrayType>
    {
        [EnableDump]
        internal readonly BitType.IOperation Definable;

        protected FeatureBase(BitType.IOperation definable) { Definable = definable; }

        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type) { return type.Feature(this); }
        ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Type.ArrayType>.Convert(Type.ArrayType type) { return this; }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }
}