using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Star : SequenceOfBitOperation, ISearchPath<IFeature,TypeType>
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return new Reni.Feature.Feature(type.Repeat); }
    }
}