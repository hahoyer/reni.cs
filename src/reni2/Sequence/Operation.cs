using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Sequence
{
    abstract class Operation
        : Defineable<Operation>
            , BitType.IOperation
    {
        int BitType.IOperation.Signature(int objectBitCount, int argsBitCount) { return Signature(objectBitCount, argsBitCount); }
        string BitType.IOperation.Name { get { return DataFunctionName; } }

        protected abstract int Signature(int objSize, int argSize);

        [DisableDumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
    }

    abstract class Operation<TTarget> : Operation
        where TTarget : Defineable
    {
        public override SearchResult FindGenericDeclarationsForType(TypeBase provider)
        {
            return provider.DeclarationsForType<TTarget>()
                ?? base.FindGenericDeclarationsForType(provider);
        }
    }
}