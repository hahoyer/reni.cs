using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal abstract class BitFeatureBase : ReniObject
    {
        protected static Result Apply(Category category, AutomaticReferenceType objectType)
        {
            return TypeBase.Void
                .Result(category, () => BitSequenceDumpPrint(objectType));
        }

        private static CodeBase BitSequenceDumpPrint(AutomaticReferenceType objectType)
        {
            var alignedSize = objectType.ValueType.Size.Align(objectType.RefAlignParam.AlignBits);
            return objectType
                .ArgCode()
                .Dereference(objectType.RefAlignParam, alignedSize)
                .DumpPrint(alignedSize);
        }
    }

    internal sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<IFeature, Sequence.SequenceType>
    {
        IFeature ISearchPath<IFeature, Sequence.SequenceType>.Convert(Sequence.SequenceType type) { return type.BitDumpPrintFeature; }
    }

    internal sealed class BitSequenceFeatureClass : BitFeatureBase, IFeature
    {
        private readonly Sequence.SequenceType _parent;

        internal BitSequenceFeatureClass(Sequence.SequenceType parent) { _parent = parent; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return Apply(category, _parent.SpawnReference(refAlignParam)); }
        TypeBase IFeature.DefiningType() { return _parent; }
    }

    internal sealed class BitFeature : BitFeatureBase, IFeature
    {
        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return Apply(category, TypeBase.Bit.SpawnReference(refAlignParam)); }
        TypeBase IFeature.DefiningType() { return TypeBase.Bit; }
    }

    internal sealed class StructReferenceFeature : ReniObject, ISearchPath<IFeature, AutomaticReferenceType>,
                                                   IFeature
    {
        [EnableDump]
        private readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        IFeature ISearchPath<IFeature, AutomaticReferenceType>.Convert(AutomaticReferenceType type)
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