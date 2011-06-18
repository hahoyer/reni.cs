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

    internal sealed class FunctionalType : TypeBase
    {
        private readonly BaseType _parent;
        private readonly FunctionalFeature _functionalFeature;

        public FunctionalType(BaseType parent, FunctionalFeature functionalFeature)
        {
            _parent = parent;
            _functionalFeature = functionalFeature;
        }

        protected override Size GetSize() { return _parent.Size; }

        internal override string DumpShort() { return "(" + _parent.DumpShort() + " " + _functionalFeature.DumpShort() + ")"; }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override TypeBase ObjectType() { return _parent; }
    }
}