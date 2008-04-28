using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    /// <summary>
    /// Special array 
    /// </summary>
    internal sealed class Sequence : TypeBase
    {
        private readonly EnableCutFeature _enableCutCutFeature;
        private readonly Array _inheritedType;

        public Sequence(TypeBase elementType, int count)
        {
            Tracer.Assert(count > 0);
            _inheritedType = elementType.CreateArray(count);
            _enableCutCutFeature = new EnableCutFeature(this);
        }

        [Node]
        public Array InheritedType { get { return _inheritedType; } }
        public override Size Size { get { return _inheritedType.Size; } }
        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }
        internal override int SequenceCount { get { return Count; } }
        internal int Count { get { return _inheritedType.Count; } }
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            var destPending = dest as Pending;
            if(destPending != null)
                return true;

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

            return base.IsConvertableToVirt(dest, conversionFeature);
        }

        internal override SearchResult<IRefFeature> SearchFromRef(Defineable defineable)
        {
            var subTrial = Element.SearchFromRefToSequence(defineable).SubTrial(Element);
            return subTrial.SearchResultDescriptor.Convert(subTrial.Feature, this);
        }

        internal protected override SearchResult<IFeature> Search(Defineable defineable)
        {
            var resultFromSequenceElement = Element.SearchFromSequence(defineable).SubTrial(Element);
            var result = resultFromSequenceElement.SearchResultDescriptor.Convert(resultFromSequenceElement.Feature,
                this);
            if(result.IsSuccessFull)
                return result;
            var resultFromSequence = defineable.SearchFromSequence();
            result = resultFromSequence
                .SearchResultDescriptor
                .Convert(resultFromSequence.Feature, this)
                .AlternativeTrial(result);
            return result;
        }

        internal protected override SearchResult<IPrefixFeature> SearchPrefix(Defineable defineable)
        {
            var resultFromSequence = Element.SearchPrefixFromSequence(defineable).SubTrial(Element);
            return resultFromSequence.SearchResultDescriptor.Convert(resultFromSequence.Feature, this);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            var destPending = dest as Pending;
            if(destPending != null)
                return Result.CreatePending(category);

            Result result;

            result = ConvertTo(category, dest as Sequence);
            if(result != null)
                return result;

            result = ConvertTo(category, dest as Aligner);
            if(result != null)
                return result;

            result = ConvertTo(category, dest as EnableCut);
            if (result != null)
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
            if (dest == null)
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
            var destructor = Element.DestructorHandler(category);
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

        internal override Result DumpPrint(Category category)
        {
            return Element.SequenceDumpPrint(category, Count);
        }

        internal override Result DestructorHandler(Category category)
        {
            return _inheritedType.DestructorHandler(category);
        }

        internal override Result MoveHandler(Category category)
        {
            return _inheritedType.MoveHandler(category);
        }

        public IFeature BitOperationFeature(SequenceOfBitOperation definable)
        {
            return new BitOperationFeatureClass(this, definable);
        }

        public IPrefixFeature BitOperationPrefixFeature(SequenceOfBitOperation definable)
        {
            return new BitOperationPrefixFeatureClass(this, definable);
        }

        public IFeature EnableCutFeatureObject()
        {
            return _enableCutCutFeature;
        }

        internal class BitOperationFeatureClass : IFeature
        {
            private readonly SequenceOfBitOperation _definable;
            private readonly Sequence _sequence;

            public BitOperationFeatureClass(Sequence sequence, SequenceOfBitOperation
                definable)
            {
                _sequence = sequence;
                _definable = definable;
            }

            Result IFeature.VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
            {
                var objResult = callObject.ConvertTo(category, _sequence);
                var rawArgResult = args.Visit(callContext, category | Category.Type);
                var argType = _sequence.Element.CreateSequence(rawArgResult.Type.SequenceCount);
                var argResult = rawArgResult.ConvertTo(argType);
                var result = new Result();
                if(category.HasSize || category.HasType || category.HasCode)
                {
                    var objBitCount = _sequence.UnrefSize.ToInt();
                    var argBitCount = argResult.Type.UnrefSize.ToInt();
                    var type =
                        _sequence.Element
                            .SequenceOperationResultType(_definable, objBitCount, argBitCount)
                            .CreateAlign(callContext.RefAlignParam.AlignBits);
                    if(category.HasSize)
                        result.Size = type.Size;
                    if(category.HasType)
                        result.Type = type;
                    if(category.HasCode)
                        result.Code = _sequence.Element.CreateSequenceOperation(_definable, objResult, type.Size,
                            argResult);
                }
                if(category.HasRefs)
                    result.Refs = objResult.Refs.Pair(argResult.Refs);
                return result;
            }
        }

        internal class BitOperationPrefixFeatureClass : IPrefixFeature
        {
            private readonly SequenceOfBitOperation _definable;
            private readonly Sequence _sequence;

            public BitOperationPrefixFeatureClass(Sequence sequence, SequenceOfBitOperation definable)
            {
                _sequence = sequence;
                _definable = definable;
            }

            public Result VisitApply(Category category, Result argResult)
            {
                var objResult = argResult.ConvertTo(_sequence);
                var result = new Result();
                if(category.HasSize || category.HasType || category.HasCode)
                {
                    if(category.HasSize)
                        result.Size = objResult.Size;
                    if(category.HasType)
                        result.Type = objResult.Type;
                    if(category.HasCode)
                        result.Code = _sequence.Element.CreateSequenceOperation(_definable, objResult);
                }
                if(category.HasRefs)
                    result.Refs = objResult.Refs;
                return result;
            }
        }

        internal class EnableCutFeature : ReniObject, IFeature
        {
            private readonly Sequence _sequence;

            public EnableCutFeature(Sequence sequence)
            {
                _sequence = sequence;
            }

            public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
            {
                if(args == null)
                    return callObject.ConvertTo(category, new EnableCut(_sequence));

                NotImplementedMethod(callContext, category, args, callObject);
                return null;
            }
        }
    }
}