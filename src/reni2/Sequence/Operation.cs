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
        : Definable
            , NumberType.IOperation
    {
        int NumberType.IOperation.Signature(int objectBitCount, int argsBitCount)
        {
            return Signature(objectBitCount, argsBitCount);
        }
        string NumberType.IOperation.Name { get { return DataFunctionName; } }

        protected abstract int Signature(int objSize, int argSize);

        [DisableDumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }
}