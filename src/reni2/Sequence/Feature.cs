using hw.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class Feature : DumpableObject, FunctionFeature.ISequenceFeature
    {
        readonly NumberType.IOperation _definable;
        readonly BitType _bitType;
        public Feature(NumberType.IOperation definable, BitType bitType)
        {
            _definable = definable;
            _bitType = bitType;
        }

        TypeBase FunctionFeature.ISequenceFeature.ResultType(int objSize, int argsSize) { return _bitType.UniqueNumber(_definable.Signature(objSize, argsSize)); }
        NumberType.IOperation FunctionFeature.ISequenceFeature.Definable { get { return _definable; } }
    }
}