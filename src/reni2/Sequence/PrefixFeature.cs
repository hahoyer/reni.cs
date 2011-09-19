using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class PrefixFeature : ReniObject, ISuffixFeature, IPrefixFeature
    {
        private readonly SequenceType _objectType;
        private readonly ISequenceOfBitPrefixOperation _definable;

        internal PrefixFeature(SequenceType objectType, ISequenceOfBitPrefixOperation definable)
        {
            _objectType = objectType;
            _definable = definable;
        }

        TypeBase IFeature.ObjectType { get { return _objectType; } }

        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam)
        {
            var resultForArg = _objectType
                .UniqueAutomaticReference(refAlignParam)
                .ArgResult(category.Typed)
                .AutomaticDereference()
                .Align(refAlignParam.AlignBits);
            return _objectType
                .Result(category, () => _objectType.BitSequenceOperation(_definable), CodeArgs.Arg)
                .ReplaceArg(resultForArg);
        }
    }
}