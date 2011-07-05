using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Sequence
{
    [Serializable]
    internal sealed class SequenceType : TypeBase
    {
        private readonly Type.Array _inheritedType;

        [DisableDump]
        internal readonly IFeature BitDumpPrintFeature;

        internal Result EnableCutFeature(Category category) { return new EnableCut(this).ArgResult(category); }

        internal IFeature Feature(FeatureBase featureBase) { return new FunctionalFeature(this, featureBase); }

        internal IPrefixFeature PrefixFeature(ISequenceOfBitPrefixOperation definable) { return new PrefixFeature(this, definable); }

        public SequenceType(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0, () => "count=" + count);
            _inheritedType = elementType.UniqueArray(count);
            BitDumpPrintFeature = new BitSequenceFeatureClass(this);
            StopByObjectId(-172);
        }

        [DisableDump]
        [UsedImplicitly]
        internal Type.Array InheritedType { get { return _inheritedType; } }

        protected override Size GetSize() { return _inheritedType.Size; }

        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }

        internal override int SequenceCount(TypeBase elementType) { return elementType == Element ? Count : 1; }

        [DisableDump]
        internal int Count { get { return _inheritedType.Count; } }

        [Node]
        [DisableDump]
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override string DumpShort() { return base.DumpShort() + "(" + Element.DumpShort() + "*" + Count + ")"; }

        internal override bool VirtualIsConvertable(SequenceType destination, ConversionParameter conversionParameter)
        {
            if (conversionParameter.IsDisableCut && Count > destination.Count)
                return false;
            return Element.IsConvertable(destination.Element, conversionParameter.DontUseConverter);
        }

        internal bool VirtualIsConvertable(TypeBase destination, ConversionParameter conversionParameter)
        {
            var destAligner = destination as Aligner;
            if(destAligner != null)
                return IsConvertable(destAligner.Parent, conversionParameter);

            NotImplementedMethod(destination,conversionParameter);
            return false;
        }

        protected override Result VirtualForceConversionFrom(Category category, TypeBase source) { return source.VirtualForceConversion(category, this); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Element.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        protected Result VirtualForceConversion(Category category, TypeBase destination)
        {
            var result = ForceConversion(category, destination as Aligner);
            if(result != null)
                return result;

            result = ForceConversion(category, destination as EnableCut);
            if(result != null)
                return result;

            NotImplementedMethod(category, destination);
            return null;
        }

        internal override Result VirtualForceConversion(Category category, SequenceType destination)
        {
            var result = ArgResult(category | Category.Type);
            if(Count > destination.Count)
                result = RemoveElementsAtEnd(category, destination.Count);

            if(Element != destination.Element)
            {
                var elementResult = Element.ForceConversion(category, destination.Element);
                NotImplementedMethod(category, destination, "result", result, "elementResult", elementResult);
                return null;
            }
            if(Count < destination.Count)
                result = destination.ExtendFrom(category, Count).ReplaceArg(result);
            return result;
        }

        private Result ForceConversion(Category category, Aligner dest)
        {
            if(dest == null)
                return null;
            return ForceConversion(category, dest.Parent).Align(dest.AlignBits);
        }

        private Result ForceConversion(Category category, EnableCut dest)
        {
            if(dest == null)
                return null;
            var result = ForceConversion(category, dest.Parent);
            return dest.Result(category, () => result.Code, () => result.Refs);
        }

        private Result ExtendFrom(Category category, int oldCount)
        {
            var oldSize = Element.Size*oldCount;
            var result = Result
                (
                    category,
                    () => Element.UniqueSequence(oldCount).ArgCode().BitCast(Size)
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
            var newType = Element.UniqueSequence(tempNewCount);
            var result = newType
                .Result
                (
                    category,
                    () => ArgCode().BitCast(newType.Size)
                );
            return result;
        }

        internal override Result Destructor(Category category) { return _inheritedType.Destructor(category); }

        internal override Result Copier(Category category) { return _inheritedType.Copier(category); }

        protected override bool VirtualIsConvertableFrom(TypeBase source, ConversionParameter conversionParameter) { return source.VirtualIsConvertable(this, conversionParameter); }
    }

}