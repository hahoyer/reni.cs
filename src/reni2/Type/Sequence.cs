using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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

        [DumpData(false)]
        internal Array InheritedType { get { return _inheritedType; } }
        [DumpData(false)]
        internal override Size Size { get { return _inheritedType.Size; } }
        internal override string DumpPrintText { get { return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")"; } }
        [Node, DumpData(false)]
        internal override int SequenceCount { get { return Count; } }
        [DumpData(false)]
        internal int Count { get { return _inheritedType.Count; } }
        [Node, DumpData(false)]
        public TypeBase Element { get { return _inheritedType.Element; } }

        internal override string DumpShort()
        {
            return "(" + Element.DumpShort() + ")sequence(" + Count + ")";
        }

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

        internal override SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable)
        {
            var subTrial = Element.SearchFromRefToSequence(defineable).SubTrial(Element);
            var result = subTrial.SearchResultDescriptor.Convert(subTrial.Feature, this);
            if(result.IsSuccessFull)
                return result;
            return base.SearchFromRef(defineable).AlternativeTrial(result);
        }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var resultFromSequenceElement = Element.SearchFromSequence(defineable).SubTrial(Element);
            var result = resultFromSequenceElement.SearchResultDescriptor.Convert(resultFromSequenceElement.Feature,this);
            if(result.IsSuccessFull)
                return result;
            var resultForSequence = defineable.SearchForSequence();
            result = resultForSequence.SearchResultDescriptor.Convert(resultForSequence.Feature, this).AlternativeTrial(result);
            if (result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
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

        internal class BitOperationPrefixFeatureClass : IPrefixFeature
        {
            private readonly SequenceOfBitOperation _definable;
            private readonly Sequence _sequence;

            public BitOperationPrefixFeatureClass(Sequence sequence, SequenceOfBitOperation definable)
            {
                _sequence = sequence;
                _definable = definable;
            }

            public Result ApplyResult(Category category, Result argResult)
            {
                var objResult = argResult.ConvertTo(_sequence);
                Result result;
                if(category.HasSize || category.HasType || category.HasCode)
                    result = objResult.Type.CreateResult(category, () => _sequence.Element.CreateSequenceOperation(_definable, objResult));
                else
                    result = new Result();
                if(category.HasRefs)
                    result.Refs = objResult.Refs;
                return result;
            }

            public Result ApplyResult(ContextBase contextBase, Category category, ICompileSyntax @object)
            {
                var objectResult = contextBase.Result(category|Category.Type,@object).ConvertTo(_sequence);
                Result result;
                if (category.HasSize || category.HasType || category.HasCode)
                    result = objectResult.Type.CreateResult(category, () => _sequence.Element.CreateSequenceOperation(_definable, objectResult));
                else
                    result = new Result();
                if (category.HasRefs)
                    result.Refs = objectResult.Refs;
                return result;
            }
        }
    }

    internal class EnableCutFeature : ReniObject, IFeature
    {
        private readonly Sequence _sequence;

        public EnableCutFeature(Sequence sequence)
        {
            _sequence = sequence;
        }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if (args != null)
                NotImplementedMethod(callContext, category, @object, args);

            return callContext.ApplyResult(category, @object, ot => ot.ConvertTo(category, new EnableCut(_sequence)));
        }
    }

    internal class BitOperationFeatureClass : ReniObject, IFeature
    {
        private readonly SequenceOfBitOperation _definable;
        private readonly Sequence _sequence;

        public BitOperationFeatureClass(Sequence sequence, SequenceOfBitOperation
            definable)
        {
            _sequence = sequence;
            _definable = definable;
        }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var trace = ObjectId == 1027 && category.HasCode;
            StartMethodDumpWithBreak(trace, callContext,category,@object,args);
            var objectSize = callContext.Type(@object).UnrefSize;
            Dump(trace, "objectSize", objectSize);
            var argsResult = callContext.ConvertToSequenceViaRef(category, args, _sequence.Element, () => objectSize.ByteAlignedSize);
            if (argsResult.IsPending)
                return Result.CreatePending(category);
            Dump(trace, "argsResult", argsResult);
            var objectResult = callContext.ConvertToSequenceViaRef(category, @object, _sequence.Element, () => argsResult.Internal.Size);
            if (objectResult.IsPending)
                return Result.CreatePending(category);
            Dump(trace, "objectResult", objectResult);

            var result = _sequence
                .Element
                .SequenceOperationResult(category, _definable, objectSize, callContext.Type(args).UnrefSize)
                ;
            Dump(trace, "result", result);

            return ReturnMethodDump(trace, result.UseWithArg(objectResult.CreateSequence(argsResult)));
        }
    }
}
