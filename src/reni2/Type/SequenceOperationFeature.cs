using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal class SequenceOperationFeature : ReniObject
                                              , IFeature
                                              , ISearchPath<IFeature, Sequence>
                                              , IFunctionalFeature

    {
        [DumpData(true)]
        private readonly ISequenceOfBitBinaryOperation _definable;

        public SequenceOperationFeature(ISequenceOfBitBinaryOperation definable)
        {
            _definable = definable;
        }

        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return this; }

        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, TypeBase objectType) { return objectType.CreateFunctionalType(this).CreateArgResult(category); }

        string IDumpShortProvider.DumpShort() { return _definable.DataFunctionName; }

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
            var type = TypeBase.CreateNumber(_definable.ResultSize(objSize, argsSize));
            return type.CreateResult(category, () => CodeBase.CreateBitSequenceOperation(type.Size, _definable, objSize, argsSize));
        }

    }
}