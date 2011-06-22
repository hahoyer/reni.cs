using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class FunctionalFeature : ReniObject, IFeature, IFunctionalFeature
    {
        private readonly BaseType _parent;

        [EnableDump]
        private readonly FeatureBase _feature;

        internal FunctionalFeature(BaseType parent, FeatureBase feature)
        {
            _parent = parent;
            _feature = feature;
        }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            return _parent
                .SpawnFunctionalType(this)
                .ArgResult(category);
        }

        string IDumpShortProvider.DumpShort() { return _feature.Definable.DataFunctionName; }

        private Result Apply(Category category, int objSize, int argsSize)
        {
            var type = _feature.ResultType(objSize, argsSize);
            return type.Result(category, () => CodeBase.BitSequenceOperation(type.Size, _feature.Definable, objSize, argsSize));
        }

        TypeBase IFeature.DefiningType() { return _parent; }

        Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            var objectResult = _parent.ObjectReferenceInCode(category | Category.Type, refAlignParam);
            var result = Apply(category, objectResult.Type.SequenceCount(_parent.Element), argsType.SequenceCount(_parent.Element));
            var convertedObjectResult = objectResult.ConvertToBitSequence(category);
            var convertedArgsResult = argsType.ConvertToBitSequence(category);
            return result.ReplaceArg(convertedObjectResult.CreateSequence(convertedArgsResult));
        }

        Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }
    }
}