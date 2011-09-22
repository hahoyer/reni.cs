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
                .Result(category, () => BitSequenceDumpPrint(objectType), CodeArgs.Arg);
        }

        private static CodeBase BitSequenceDumpPrint(AutomaticReferenceType objectType)
        {
            var alignedSize = objectType.ValueType.Size.Align(objectType.RefAlignParam.AlignBits);
            return objectType
                .ArgCode()
                .Dereference(objectType.RefAlignParam, alignedSize)
                .DumpPrintNumber(alignedSize);
        }
    }

    internal sealed class BitSequenceFeature :
        ReniObject,
        ISearchPath<ISuffixFeature, Sequence.SequenceType>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, Sequence.SequenceType>.Convert(Sequence.SequenceType type) { return type.BitDumpPrintFeature; }
    }

    internal sealed class BitSequenceFeatureClass : BitFeatureBase, ISuffixFeature
    {
        private readonly Sequence.SequenceType _parent;

        internal BitSequenceFeatureClass(Sequence.SequenceType parent) { _parent = parent; }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return Apply(category, _parent.UniqueAutomaticReference(refAlignParam)); }
        TypeBase IFeature.ObjectType { get { return _parent; } }
    }

    internal sealed class BitFeature : BitFeatureBase, ISuffixFeature
    {
        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return Apply(category, TypeBase.Bit.UniqueAutomaticReference(refAlignParam)); }
        TypeBase IFeature.ObjectType { get { return TypeBase.Bit; } }
    }

    internal sealed class StructReferenceFeature : ReniObject, ISearchPath<ISuffixFeature, AutomaticReferenceType>,
                                                   ISuffixFeature
    {
        [EnableDump]
        private readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        ISuffixFeature ISearchPath<ISuffixFeature, AutomaticReferenceType>.Convert(AutomaticReferenceType type)
        {
            Tracer.Assert(type.RefAlignParam == _structureType.RefAlignParam);
            return this;
        }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam)
        {
            return _structureType
                .Structure
                .DumpPrintResultViaContextReference(category);
        }

        TypeBase IFeature.ObjectType { get { return _structureType; } }
    }
}