// #pragma warning disable 649
using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Feature;
using Reni.Feature.DumpPrint;

#pragma warning disable 1911

namespace Reni.Type
{
    /// <summary>
    /// Special array 
    /// </summary>
    [Serializable]
    internal sealed class Sequence : TypeBase
    {
        private readonly EnableCutFeature _enableCutCutFeature;
        private readonly Array _inheritedType;
        internal readonly IFeature BitDumpPrintFeature;

        internal IFeature Feature(SequenceFeatureBase sequenceFeatureBase) { return new FeatureClass(this, sequenceFeatureBase); }

        internal IPrefixFeature PrefixFeature(ISequenceOfBitPrefixOperation definable) { return new PrefixFeatureClass(this, definable); }

        private class FeatureClass : ReniObject, IFeature, IFunctionalFeature
        {
            private readonly Sequence _parent;
            private readonly SequenceFeatureBase _sequenceFeature;

            protected internal FeatureClass(Sequence parent, SequenceFeatureBase sequenceFeature)
            {
                _parent = parent;
                _sequenceFeature = sequenceFeature;
            }

            Result IFeature.Apply(Category category) { return _parent.CreateFunctionalType(this).CreateArgResult(category); }

            string IDumpShortProvider.DumpShort() { return _sequenceFeature.Definable.DataFunctionName; }

            private Result Apply(Category category, int objSize, int argsSize)
            {
                var type = _sequenceFeature.ResultType(objSize, argsSize);
                return type.CreateResult(category, () => CodeBase.CreateBitSequenceOperation(type.Size, _sequenceFeature.Definable, objSize, argsSize));
            }

            TypeBase IFeature.DefiningType() { return _parent; }

            Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
            {
                var objectResult = functionalResult.StripFunctional();
                var result = Apply(category, objectResult.Type.SequenceCount, argsResult.Type.SequenceCount);
                var convertedObjectResult = objectResult.ConvertToBitSequence(category);
                var convertedArgsResult = argsResult.ConvertToBitSequence(category);
                return result.UseWithArg(convertedObjectResult.CreateSequence(convertedArgsResult));
            }
        }

        private class PrefixFeatureClass : ReniObject, IFeature, IPrefixFeature
        {
            private readonly Sequence _parent;
            private readonly ISequenceOfBitPrefixOperation _definable;

            protected internal PrefixFeatureClass(Sequence parent, ISequenceOfBitPrefixOperation definable)
            {
                _parent = parent;
                _definable = definable;
            }

            IFeature IPrefixFeature.Feature { get { return this; } }

            TypeBase IFeature.DefiningType() { return _parent; }

            Result IFeature.Apply(Category category)
            {
                return Apply(category, _parent.UnrefSize)
                    .UseWithArg(_parent.ConvertToBitSequence(category));
            }

            private Result Apply(Category category, Size objSize)
            {
                var type = CreateNumber(objSize.ToInt());
                return type.CreateResult(category,
                                         () => CodeBase.CreateBitSequenceOperation(type.Size, _definable, objSize));
            }
        }

        public Sequence(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0, "count=" + count);
            _inheritedType = elementType.CreateArray(count);
            _enableCutCutFeature = new EnableCutFeature(this);
            BitDumpPrintFeature = new BitSequenceFeatureClass(this);
            StopByObjectId(172);
        }

        [DumpData(false), UsedImplicitly]
        internal Array InheritedType { get { return _inheritedType; } }

        protected override Size GetSize() { return _inheritedType.Size; }

        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }

        internal override bool IsValidRefTarget() { return _inheritedType.IsValidRefTarget(); }

        [Node, DumpData(false)]
        internal override int SequenceCount { get { return Count; } }

        [DumpData(false)]
        internal int Count { get { return _inheritedType.Count; } }

        [Node, DumpData(false)]
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override string DumpShort() { return "(" + Element.DumpShort() + ")sequence(" + Count + ")"; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            var destSequence = dest as Sequence;
            if(destSequence != null)
            {
                if(conversionFeature.IsDisableCut && Count > destSequence.Count)
                    return false;
                return Element.IsConvertableTo(destSequence.Element, conversionFeature.DontUseConverter);
            }

            var destAligner = dest as Aligner;
            if(destAligner != null)
                return IsConvertableTo(destAligner.Parent, conversionFeature);

            return base.IsConvertableToImplementation(dest, conversionFeature);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Element.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var result = ConvertTo(category, dest as Sequence);
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

        private Result ConvertTo(Category category, Sequence dest)
        {
            if(dest == null)
                return null;

            var result = CreateArgResult(category);
            if(Count > dest.Count)
                result = RemoveElementsAtEnd(category, dest.Count);

            if(Element != dest.Element)
            {
                var elementResult = Element.ConvertTo(category, dest.Element);
                NotImplementedMethod(category, dest, "result", result, "elementResult", elementResult);
                return null;
            }
            if(Count < dest.Count)
                result = dest.ExtendFrom(category, Count).UseWithArg(result);
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
            return dest.CreateResult(category, () => result.Code, () => result.Refs);
        }

        private Result ExtendFrom(Category category, int oldCount)
        {
            var oldSize = Element.Size*oldCount;
            var result = CreateResult
                (
                category,
                () => CodeBase.CreateArg(oldSize).CreateBitCast(Size)
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
            var newType = Element.CreateSequence(tempNewCount);
            var result = newType
                .CreateResult
                (
                category,
                () => CodeBase.CreateArg(Size).CreateBitCast(newType.Size)
                );
            return result;
        }

        internal override Result Destructor(Category category) { return _inheritedType.Destructor(category); }

        internal override Result Copier(Category category) { return _inheritedType.Copier(category); }

        public IFeature EnableCutFeature { get { return _enableCutCutFeature; } }
    }
}