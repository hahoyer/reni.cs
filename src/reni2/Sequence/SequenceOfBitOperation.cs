using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class SequenceOfBitOperation :
        Defineable,
        ISearchPath<ISearchPath<IFeature, SequenceType>, Bit>,
        ISequenceOfBitBinaryOperation
    {
        ISearchPath<IFeature, SequenceType> ISearchPath<ISearchPath<IFeature, SequenceType>, Bit>.Convert(Bit type)
        {
            if(IsCompareOperator)
                return new CompareFeature(this);
            return new Feature(this);
        }

        [DumpExcept(true)]
        bool ISequenceOfBitBinaryOperation.IsCompareOperator { get { return IsCompareOperator; } }

        [DisableDump]
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }

        [DisableDump]
        string ISequenceOfBitBinaryOperation.CSharpNameOfDefaultOperation { get { return CSharpNameOfDefaultOperation; } }

        int ISequenceOfBitBinaryOperation.ResultSize(int objBitCount, int argBitCount) { return ResultSize(objBitCount, argBitCount); }

        protected abstract int ResultSize(int objSize, int argSize);

        [DisableDump]
        protected virtual string CSharpNameOfDefaultOperation { get { return Name; } }

        [DumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
    }
}