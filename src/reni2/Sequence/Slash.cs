using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Slash : SequenceOfBitOperation, ISearchPath<ISuffixFeature,TypeType>
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.DivideSize(objSize, argSize); }
        ISuffixFeature ISearchPath<ISuffixFeature, TypeType>.Convert(TypeType type) { return Extension.Feature(type.Split); }
    }
}