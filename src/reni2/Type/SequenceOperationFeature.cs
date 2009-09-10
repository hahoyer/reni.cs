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

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, Result objectResult) { return objectResult.CreateFunctionalResult(category, this); }

        string IDumpShortProvider.DumpShort() { return _definable.DataFunctionName; }

        Result IFunctionalFeature.Apply(Category category, Result objectResult, Result argsResult)
        {
            var result = Apply(category, objectResult.Type.SequenceCount, argsResult.Type.SequenceCount);
            var convertedObjectResult = objectResult.ConvertToSequence(category);
            var convertedArgsResult = argsResult.ConvertToSequence(category);
            return result.UseWithArg(convertedObjectResult.CreateSequence(convertedArgsResult));
        }

        private Result Apply(Category category, int objSize, int argsSize)
        {
            var type = TypeBase.CreateNumber(_definable.ResultSize(objSize, argsSize));
            return type.CreateResult(category, () => CodeBase.CreateBitSequenceOperation(type.Size, _definable, objSize, argsSize));
        }

    }
}