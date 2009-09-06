using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

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

        public Sequence(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0, "count=" + count);
            _inheritedType = elementType.CreateArray(count);
            _enableCutCutFeature = new EnableCutFeature(this);
        }

        [DumpData(false)]
        internal Array InheritedType { get { return _inheritedType; } }

        protected override Size GetSize() { return _inheritedType.Size; }

        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }

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

    [Serializable]
    internal class EnableCutFeature : ReniObject, IFeature
    {
        private readonly Sequence _sequence;

        public EnableCutFeature(Sequence sequence) { _sequence = sequence; }

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, Result objectResult)
        {
            return objectResult.Type.ConvertTo(category, new EnableCut(_sequence))
                .UseWithArg(objectResult);
        }
    }

    [Serializable]
    internal class SequenceOperationFeature : ReniObject
                                              , IFeature
                                              , ISearchPath<IFeature, Sequence>
                                              , IFunctionalFeature

    {
        [DumpData(true)]
        private readonly ISequenceOfBitBinaryOperation _definable;

        private readonly Bit _bit;

        public SequenceOperationFeature(Bit bit, ISequenceOfBitBinaryOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return this; }

        private Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                   ICompileSyntax args) { return _bit.ApplySequenceOperation(_definable, callContext, category, @object, args); }

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, Result objectResult)
        {
            return objectResult.CreateFunctionalResult(category, this);
        }

        string IDumpShortProvider.DumpShort() { return _definable.DataFunctionName; }
    }

    internal interface IFunctionalFeature:IDumpShortProvider {}

    [Serializable]
    internal class SequenceOperationPrefixFeature : ReniObject
                                                    , IPrefixFeature
                                                    , ISearchPath<IPrefixFeature, Sequence>
                                                    , IFeature
    {
        private readonly ISequenceOfBitPrefixOperation _definable;
        private readonly Bit _bit;

        public SequenceOperationPrefixFeature(Bit bit, ISequenceOfBitPrefixOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, Result objectResult)
        {
            NotImplementedMethod(category, objectResult);
            return null;
        }

        IPrefixFeature ISearchPath<IPrefixFeature, Sequence>.Convert(Sequence type) { return this; }
        IFeature IPrefixFeature.Feature { get { return this; } }
    }
}