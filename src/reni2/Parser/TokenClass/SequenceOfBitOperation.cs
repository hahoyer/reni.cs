using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation :
        Defineable,
        ISearchPath<ISearchPath<IInfixFeature, Sequence>, Bit>,
        ISequenceOfBitBinaryOperation
    {
        ISearchPath<IInfixFeature, Sequence> ISearchPath<ISearchPath<IInfixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return new SequenceOperationFeature(type, this);
        }

        bool ISequenceOfBitBinaryOperation.IsCompareOperator { get { return IsCompareOperator; } }
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }
        string ISequenceOfBitBinaryOperation.CSharpNameOfDefaultOperation { get { return CSharpNameOfDefaultOperation; } }

        int ISequenceOfBitBinaryOperation.ResultSize(int objBitCount, int argBitCount)
        {
            return ResultSize(objBitCount, argBitCount);
        }

        protected abstract int ResultSize(int objSize, int argSize);

        protected virtual string CSharpNameOfDefaultOperation { get { return Name; } }
        protected virtual bool IsCompareOperator { get { return false; } }
    }

    [Token("+")]
    [Token("-")]
    internal sealed class Sign :
        SequenceOfBitOperation,
        ISearchPath<ISearchPath<IPrefixFeature, Sequence>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        ISearchPath<IPrefixFeature, Sequence> ISearchPath<ISearchPath<IPrefixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return new SequenceOperationPrefixFeature(type, this);
        }
        
        int ISequenceOfBitPrefixOperation.ResultSize(int objBitCount) { return objBitCount; }
        string ISequenceOfBitOperation.CSharpNameOfDefaultOperation { get { return Name; } }
        string ISequenceOfBitOperation.DataFunctionName { get { return DataFunctionName; } }

        Result ISequenceOfBitOperation.SequenceOperationResult(Category category, Size objSize)
        {
            return Bit.PrefixSequenceOperationResult(category, this, objSize);
        }

        protected override int ResultSize(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
    }

    [Token("*")]
    internal sealed class Star : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
    }

    [Token("/")]
    internal sealed class Slash : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.DivideSize(objSize, argSize); }
    }
}