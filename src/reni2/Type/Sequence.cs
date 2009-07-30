using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
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

        internal new Result DumpPrint(Category category) { return Element.SequenceDumpPrint(category, Count); }

        internal override Result Destructor(Category category) { return _inheritedType.Destructor(category); }

        internal override Result Copier(Category category) { return _inheritedType.Copier(category); }

        public SequenceOperationPrefixFeature BitOperationPrefixFeature(ISequenceOfBitPrefixOperation definable) { return new SequenceOperationPrefixFeature(this, definable); }

        public IInfixFeature EnableCutFeature { get { return _enableCutCutFeature; } }
        public IInfixFeature BitOperationFeature(ISequenceOfBitDumpPrint feature) { return new SequenceOfBitDumpPrintFeature(this, feature); }
    }

    internal class SequenceOfBitDumpPrintFeature : ReniObject, IInfixFeature
    {
        private readonly Sequence _sequence;
        private readonly ISequenceOfBitDumpPrint _feature;

        public SequenceOfBitDumpPrintFeature(Sequence sequence, ISequenceOfBitDumpPrint feature)
        {
            _sequence = sequence;
            _feature = feature;
        }

        Result IInfixFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);

            return _sequence.Element.ApplySequenceOperation(_feature, callContext, category, @object);
        }
    }

    [Serializable]
    internal class EnableCutFeature : ReniObject, IInfixFeature
    {
        private readonly Sequence _sequence;

        public EnableCutFeature(Sequence sequence) { _sequence = sequence; }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);

            return callContext.ApplyResult(category, @object, ot => ot.ConvertTo(category, new EnableCut(_sequence)));
        }
    }

    [Serializable]
    internal class SequenceOperationFeature : ReniObject, IInfixFeature, IConverter<IInfixFeature, Sequence>
    {
        private readonly ISequenceOfBitBinaryOperation _definable;
        private readonly Bit _bit;

        public SequenceOperationFeature(Bit bit, ISequenceOfBitBinaryOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        Result IInfixFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args) { return _bit.ApplySequenceOperation(_definable, callContext, category, @object, args); }

        public IInfixFeature Convert(Sequence type) { return this; }
    }

    [Serializable]
    internal class SequenceOperationPrefixFeature : ReniObject, IPrefixFeature
    {
        private readonly ISequenceOfBitPrefixOperation _definable;
        private readonly Bit _bit;

        public SequenceOperationPrefixFeature(Bit bit, ISequenceOfBitPrefixOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        Result IPrefixFeature.ApplyResult(ContextBase callCallContext, Category category, ICompileSyntax @object)
        {
            return _bit.ApplySequenceOperation(_definable, callCallContext, category, @object);
        }
    }
}