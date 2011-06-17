using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Sequence;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, int objSize) { return TypeBase.Void.Result(category, () => CodeBase.BitSequenceDumpPrint(objSize)); }
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

        public BitSequenceFeatureClass(BaseType parent) { _parent = parent; }

        Result IFeature.Apply(Category category)
        {
            return
                Apply(category, _parent.SequenceCount(TypeBase.Bit))
                    .ReplaceArg(_parent.ArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return _parent; }
    }

    internal sealed class BitFeature : BitFeatureBase, IFeature
    {
        Result IFeature.Apply(Category category)
        {
            return
                Apply(category, 1)
                    .ReplaceArg(TypeBase.Bit.ArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return TypeBase.Bit; }
    }

    internal sealed class StructReferenceFeature : ReniObject, ISearchPath<IFeature, Reference>, IFeature
    {
        [EnableDump]
        private readonly Struct.AccessPointType _accessPointType;

        public StructReferenceFeature(Struct.AccessPointType accessPointType) { _accessPointType = accessPointType; }

        IFeature ISearchPath<IFeature, Reference>.Convert(Reference type)
        {
            Tracer.Assert(type.RefAlignParam == _accessPointType.RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category) { return _accessPointType.DumpPrintResult(category); }

        TypeBase IFeature.DefiningType() { return _accessPointType; }
    }
}