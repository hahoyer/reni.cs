using System.Collections.Generic;
using hw.DebugFormatter;
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
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);

    }

    abstract class TransformationOperation
        : Operation
            , NumberType.ITransformation
    {
        int NumberType.ITransformation.Signature(int objectBitCount, int argsBitCount) => Signature(objectBitCount, argsBitCount);
        protected abstract int Signature(int objSize, int argSize);
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
    }
}