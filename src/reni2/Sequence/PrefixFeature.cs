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
    internal sealed class PrefixFeature : ReniObject, IFeature, IPrefixFeature
    {
        private readonly BaseType _parent;
        private readonly ISequenceOfBitPrefixOperation _definable;

        internal PrefixFeature(BaseType parent, ISequenceOfBitPrefixOperation definable)
        {
            _parent = parent;
            _definable = definable;
        }

        IFeature IPrefixFeature.Feature { get { return this; } }

        TypeBase IFeature.DefiningType() { return _parent; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            return Apply(category, _parent.UnrefSize, refAlignParam)
                .ReplaceArg(_parent.ConvertToBitSequence(category));
        }

        private Result Apply(Category category, Size objSize, RefAlignParam refAlignParam)
        {
            var type = TypeBase.Number(objSize.ToInt());
            return type
                .Result
                (category
                , () => CodeBase.BitSequenceOperation(type.Size, _definable, type.SpawnReference(refAlignParam)));
        }
    }
}