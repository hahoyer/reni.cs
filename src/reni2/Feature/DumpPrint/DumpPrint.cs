using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, int objSize) { return TypeBase.CreateVoid.CreateResult(category, () => CodeBase.CreateBitSequenceDumpPrint(objSize)); }
    }

    internal sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<IFeature, Sequence>
    {
        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return type.BitDumpPrintFeature; }
    }

    internal class BitSequenceFeatureClass : BitFeatureBase, IFeature
    {
        private readonly Sequence _parent;

        public BitSequenceFeatureClass(Sequence parent) { _parent = parent; }

        Result IFeature.Apply(Category category)
        {
            return
                Apply(category, _parent.GetSequenceCount(TypeBase.CreateBit))
                    .ReplaceArg(_parent.CreateArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return _parent; }
    }

    internal sealed class BitFeature : BitFeatureBase, IFeature
    {
        Result IFeature.Apply(Category category)
        {
            return
                Apply(category, 1)
                    .ReplaceArg(TypeBase.CreateBit.CreateArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return TypeBase.CreateBit; }
    }

    internal class StructReferenceFeature : ReniObject, ISearchPath<IFeature, Type.Reference>, IFeature
    {
        [DumpData(true)]
        private readonly Struct.Type _type;
        public StructReferenceFeature(Struct.Type type) { _type = type; }
        IFeature ISearchPath<IFeature, Type.Reference>.Convert(Type.Reference type)
        {
            Tracer.Assert(type.RefAlignParam == _type.RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category)
        {
            return _type.CreateDumpPrintResult(category);
        }

        TypeBase IFeature.DefiningType()
        {
            return _type;
        }
    }
}