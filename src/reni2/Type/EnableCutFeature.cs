using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;

namespace Reni.Type
{
    [Serializable]
    internal class EnableCutFeature : ReniObject, ISuffixFeature
    {
        private readonly Sequence.SequenceType _sequenceType;
        public EnableCutFeature(Sequence.SequenceType sequenceType) { _sequenceType = sequenceType; }
        TypeBase IFeature.ObjectType { get { return _sequenceType; } }
        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return new EnableCut(_sequenceType).ArgResult(category); }
    }
}