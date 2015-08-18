using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Numeric
{
    abstract class Operation
        : Definable
            , NumberType.IOperation
    {
        string NumberType.IOperation.Name => DataFunctionName;
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    abstract class TransformationOperation
        : Operation
            , NumberType.ITransformation
    {
        int NumberType.ITransformation.Signature(int objectBitCount, int argsBitCount) => Signature(objectBitCount, argsBitCount);
        protected abstract int Signature(int objSize, int argSize);
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}