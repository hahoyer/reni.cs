using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal class SequenceOperationPrefixFeature : ReniObject
                                                    , IPrefixFeature
                                                    , ISearchPath<IPrefixFeature, Sequence>
                                                    , IFeature
    {
        [DumpData(true)]
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
            var result = Apply(category,objectResult.Type.UnrefSize);
            return result.UseWithArg(objectResult.ConvertToSequence(category));
        }

        IPrefixFeature ISearchPath<IPrefixFeature, Sequence>.Convert(Sequence type) { return this; }
        IFeature IPrefixFeature.Feature { get { return this; } }

        private Result Apply(Category category, Size objSize)
        {
            var type = TypeBase.CreateNumber(objSize.ToInt());
            return type.CreateResult(category, 
                () => CodeBase.CreateBitSequenceOperation(type.Size, _definable, objSize));
        }

    }
}