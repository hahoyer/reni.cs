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
        protected static Result Apply(Category category, int objSize) { return TypeBase.CreateVoid.CreateResult(category, () => CodeBase.CreateBitSequenceDumpPrint(objSize)); }
    }

    internal class VoidFeature : ReniObject, IFeature
    {
        Result IFeature.Apply(Category category) { return TypeBase.CreateVoidResult(category); }
        TypeBase IFeature.DefiningType() { return TypeBase.CreateVoid; }
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
                    .UseWithArg(_parent.CreateArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return _parent; }
    }

    internal class StructFeature : ReniObject, IFeature
    {
        [DumpData(true)]
        private readonly Struct.Type _parent;

        public StructFeature(Struct.Type parent) { _parent = parent; }

        TypeBase IFeature.DefiningType() { return _parent; }

        Result IFeature.Apply(Category category) { return _parent.DumpPrint(category); }
    }

    internal sealed class BitFeature : BitFeatureBase, IFeature
    {
        Result IFeature.Apply(Category category)
        {
            return
                Apply(category, 1)
                    .UseWithArg(TypeBase.CreateBit.CreateArgResult(category).Align(BitsConst.SegmentAlignBits));
        }

        TypeBase IFeature.DefiningType() { return TypeBase.CreateBit; }
    }
}