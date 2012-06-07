using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Star : SequenceOfBitOperation, ISearchPath<ISuffixFeature,TypeType>
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
        ISuffixFeature ISearchPath<ISuffixFeature, TypeType>.Convert(TypeType type) { return Feature(type.Repeat); }
    }
}