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
        private readonly SequenceType _parent;
        private readonly ISequenceOfBitPrefixOperation _definable;

        internal PrefixFeature(SequenceType parent, ISequenceOfBitPrefixOperation definable)
        {
            _parent = parent;
            _definable = definable;
        }

        IFeature IPrefixFeature.Feature { get { return this; } }

        TypeBase IFeature.ObjectType { get { return _parent; } }

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
                , () => BitSequenceOperation(type.Size, _definable, type.SpawnAutomaticReference(refAlignParam)));
        }

        private static CodeBase BitSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, AutomaticReferenceType objectType)
        {
            return objectType
                .ArgCode()
                .Dereference(objectType.RefAlignParam, objectType.ValueType.Size.ByteAlignedSize)
                .BitSequenceOperation(feature, size);
        }
    }
}