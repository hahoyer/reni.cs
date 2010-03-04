using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation :
        Defineable,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>,
        ISequenceOfBitBinaryOperation
    {
        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type)
        {
            if(IsCompareOperator)
                return new SequenceCompareFeature(this);
            return new SequenceFeature(this);
        }

        [DumpExcept(true)]
        bool ISequenceOfBitBinaryOperation.IsCompareOperator { get { return IsCompareOperator; } }
        [DumpData(false)]
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }
        [DumpData(false)]
        string ISequenceOfBitBinaryOperation.CSharpNameOfDefaultOperation { get { return CSharpNameOfDefaultOperation; } }

        int ISequenceOfBitBinaryOperation.ResultSize(int objBitCount, int argBitCount) { return ResultSize(objBitCount, argBitCount); }

        protected abstract int ResultSize(int objSize, int argSize);

        [DumpData(false)]
        protected virtual string CSharpNameOfDefaultOperation { get { return Name; } }
        [DumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
    }

    internal sealed class Sign :
        SequenceOfBitOperation,
        ISearchPath<ISearchPath<IPrefixFeature, Sequence>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        ISearchPath<IPrefixFeature, Sequence> ISearchPath<ISearchPath<IPrefixFeature, Sequence>, Bit>.Convert(Bit type) { return new SequenceOperationPrefixFeature(type, this); }

        [DumpData(false)]
        string ISequenceOfBitPrefixOperation.CSharpNameOfDefaultOperation { get { return Name; } }
        [DumpData(false)]
        string ISequenceOfBitPrefixOperation.DataFunctionName { get { return DataFunctionName; } }
        public Result SequenceOperationResult(Category category, Size objSize) { throw new NotImplementedException(); }

        protected override int ResultSize(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
    }

    internal sealed class Star : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
    }

    internal sealed class Slash : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.DivideSize(objSize, argSize); }
    }
}