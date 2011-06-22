using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, int objSize, RefAlignParam refAlignParam) { return TypeBase.Void.Result(category, () => CodeBase.BitSequenceDumpPrint(objSize, refAlignParam)); }
    }

    internal sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<IFeature, BaseType>
    {
        IFeature ISearchPath<IFeature, BaseType>.Convert(BaseType type) { return type.BitDumpPrintFeature; }
    }

    internal sealed class BitSequenceFeatureClass : BitFeatureBase, IFeature
    {
        private readonly BaseType _parent;

        internal BitSequenceFeatureClass(BaseType parent) { _parent = parent; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return Apply(category, _parent.SequenceCount(TypeBase.Bit), refAlignParam); }
        TypeBase IFeature.DefiningType() { return _parent; }
    }

    internal sealed class BitFeature : BitFeatureBase, IFeature
    {
        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return Apply(category, 1, refAlignParam); }
        TypeBase IFeature.DefiningType() { return TypeBase.Bit; }
    }

    internal sealed class StructReferenceFeature : ReniObject, ISearchPath<IFeature, Reference>, IFeature
    {
        [EnableDump]
        private readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        IFeature ISearchPath<IFeature, Reference>.Convert(Reference type)
        {
            Tracer.Assert(type.RefAlignParam == _structureType.RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            return _structureType
                .Structure
                .DumpPrintResultFromContextReference(category);
        }

        TypeBase IFeature.DefiningType() { return _structureType; }
    }
}