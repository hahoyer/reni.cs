using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    internal abstract class SequenceOperationFeatureBase : ReniObject, IFeature, ISearchPath<IFeature, Sequence>, IFunctionalFeature
    {
        [DumpData(true)]
        protected readonly ISequenceOfBitBinaryOperation Definable;

        protected SequenceOperationFeatureBase(ISequenceOfBitBinaryOperation definable)
        {
            Definable = definable;
        }

        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return this; }
        Result IFeature.Apply(Category category, TypeBase objectType) { return objectType.CreateFunctionalType(this).CreateArgResult(category); }
        string IDumpShortProvider.DumpShort() { return Definable.DataFunctionName; }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var objectResult = functionalResult.StripFunctional();
            var result = Apply(category, objectResult.Type.SequenceCount, argsResult.Type.SequenceCount);
            var convertedObjectResult = objectResult.ConvertToBitSequence(category);
            var convertedArgsResult = argsResult.ConvertToBitSequence(category);
            return result.UseWithArg(convertedObjectResult.CreateSequence(convertedArgsResult));
        }

        private Result Apply(Category category, int objSize, int argsSize)
        {
            var type = ResultType(objSize, argsSize);
            return type.CreateResult(category, () => CodeBase.CreateBitSequenceOperation(type.Size, Definable, objSize, argsSize));
        }

        protected abstract TypeBase ResultType(int objSize, int argsSize);
    }

    internal class SequenceOperationFeature : SequenceOperationFeatureBase

    {
        public SequenceOperationFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable) {
            }

        protected override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.CreateNumber(Definable.ResultSize(objSize, argsSize));
        }
    }
    internal class SequenceCompareOperationFeature : SequenceOperationFeatureBase
    {
        public SequenceCompareOperationFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable)
        {
        }

        protected override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.CreateBit;
        }
    }
}