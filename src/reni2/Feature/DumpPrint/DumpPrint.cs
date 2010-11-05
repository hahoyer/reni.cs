using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, int objSize) { return TypeBase.Void.Result(category, () => CodeBase.BitSequenceDumpPrint(objSize)); }
    }

    internal sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<IFeature, Type.Sequence>
    {
        IFeature ISearchPath<IFeature, Type.Sequence>.Convert(Type.Sequence type) { return type.BitDumpPrintFeature; }
    }

    internal sealed class BitSequenceFeatureClass : BitFeatureBase, IFeature
    {
        private readonly Type.Sequence _parent;

        public BitSequenceFeatureClass(Type.Sequence parent) { _parent = parent; }

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
        [IsDumpEnabled(true)]
        private readonly Struct.Type _type;

        public StructReferenceFeature(Struct.Type type) { _type = type; }

        IFeature ISearchPath<IFeature, Reference>.Convert(Reference type)
        {
            Tracer.Assert(type.RefAlignParam == _type.RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category) { return _type.CreateDumpPrintResult(category); }

        TypeBase IFeature.DefiningType() { return _type; }
    }
}