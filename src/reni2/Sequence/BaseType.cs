using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Sequence
{
    [Serializable]
    internal sealed class BaseType : TypeBase
    {
        private readonly Type.Array _inheritedType;

        [DisableDump]
        internal readonly IFeature BitDumpPrintFeature;

        private readonly DictionaryEx<FunctionalFeature, TypeBase> _functionalTypeCache;

        internal Result EnableCutFeature(Category category) { return new EnableCut(this).ArgResult(category); }

        internal IFeature Feature(FeatureBase featureBase) { return new FunctionalFeature(this, featureBase); }

        internal IPrefixFeature PrefixFeature(ISequenceOfBitPrefixOperation definable) { return new PrefixFeature(this, definable); }

        public BaseType(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0, () => "count=" + count);
            _inheritedType = elementType.Array(count);
            BitDumpPrintFeature = new BitSequenceFeatureClass(this);
            StopByObjectId(-172);
            _functionalTypeCache = new DictionaryEx<FunctionalFeature, TypeBase>(functionalFeature => new FunctionalFeatureType<BaseType, FunctionalFeature>(this, functionalFeature));
        }

        [DisableDump]
        [UsedImplicitly]
        internal Type.Array InheritedType { get { return _inheritedType; } }

        internal TypeBase SpawnFunctionalType(FunctionalFeature functionalFeature) { return _functionalTypeCache.Find(functionalFeature); }

        protected override Size GetSize() { return _inheritedType.Size; }

        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }

        internal override int SequenceCount(TypeBase elementType) { return elementType == Element ? Count : 1; }

        [DisableDump]
        internal int Count { get { return _inheritedType.Count; } }

        [Node]
        [DisableDump]
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override string DumpShort() { return "(" + Element.DumpShort() + ")sequence(" + Count + ")"; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            var destSequence = dest as BaseType;
            if(destSequence != null)
            {
                if(conversionParameter.IsDisableCut && Count > destSequence.Count)
                    return false;
                return Element.IsConvertableTo(destSequence.Element, conversionParameter.DontUseConverter);
            }

            var destAligner = dest as Aligner;
            if(destAligner != null)
                return IsConvertableTo(destAligner.Parent, conversionParameter);

            return base.IsConvertableToImplementation(dest, conversionParameter);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Element.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var result = ConvertTo(category, dest as BaseType);
            if(result != null)
                return result;

            result = ConvertTo(category, dest as Aligner);
            if(result != null)
                return result;

            result = ConvertTo(category, dest as EnableCut);
            if(result != null)
                return result;

            NotImplementedMethod(category, dest);
            return null;
        }

        private Result ConvertTo(Category category, BaseType dest)
        {
            if(dest == null)
                return null;

            var result = ArgResult(category);
            if(Count > dest.Count)
                result = RemoveElementsAtEnd(category, dest.Count);

            if(Element != dest.Element)
            {
                var elementResult = Element.ConvertTo(category, dest.Element);
                NotImplementedMethod(category, dest, "result", result, "elementResult", elementResult);
                return null;
            }
            if(Count < dest.Count)
                result = dest.ExtendFrom(category, Count).ReplaceArg(result);
            return result;
        }

        private Result ConvertTo(Category category, Aligner dest)
        {
            if(dest == null)
                return null;
            return ConvertTo(category, dest.Parent).Align(dest.AlignBits);
        }

        private Result ConvertTo(Category category, EnableCut dest)
        {
            if(dest == null)
                return null;
            var result = ConvertTo(category, dest.Parent);
            return dest.Result(category, () => result.Code, () => result.Refs);
        }

        private Result ExtendFrom(Category category, int oldCount)
        {
            var oldSize = Element.Size*oldCount;
            var result = Result
                (
                    category,
                    () => CodeBase.Arg(oldSize).CreateBitCast(Size)
                );
            return result;
        }

        private Result RemoveElementsAtEnd(Category category, int newCount)
        {
            var destructor = Element.Destructor(category);
            if(!destructor.IsEmpty)
            {
                NotImplementedMethod(category, newCount, "destructor", destructor);
                return null;
            }
            var tempNewCount = Math.Min(Count, newCount);
            var newType = Element.Sequence(tempNewCount);
            var result = newType
                .Result
                (
                    category,
                    () => CodeBase.Arg(Size).CreateBitCast(newType.Size)
                );
            return result;
        }

        internal override Result Destructor(Category category) { return _inheritedType.Destructor(category); }

        internal override Result Copier(Category category) { return _inheritedType.Copier(category); }

        internal Result ObjectReferenceInCode(Category category, RefAlignParam refAlignParam) { return Reference(refAlignParam).ObjectReferenceInCode(category); }

    }

}