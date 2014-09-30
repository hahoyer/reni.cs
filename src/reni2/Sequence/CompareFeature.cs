using hw.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class CompareFeature : DumpableObject, FunctionFeature.ISequenceFeature
    {
        readonly NumberType.IOperation _definable;
        readonly BitType _bitType;
        internal CompareFeature(NumberType.IOperation definable, BitType bitType)
        {
            _definable = definable;
            _bitType = bitType;
        }

        NumberType.IOperation FunctionFeature.ISequenceFeature.Definable { get { return _definable; } }
        TypeBase FunctionFeature.ISequenceFeature.ResultType(int objSize, int argsSize) { return _bitType; }
    }
}