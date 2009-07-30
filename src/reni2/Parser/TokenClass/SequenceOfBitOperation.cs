using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation :
        Defineable,
        IConverter<IConverter<IInfixFeature, Sequence>, Bit>,
        ISequenceOfBitBinaryOperation
    {
        IConverter<IInfixFeature, Sequence> IConverter<IConverter<IInfixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return new SequenceOperationFeature(type, this);
        }

        bool ISequenceOfBitBinaryOperation.IsCompareOperator { get { return IsCompareOperator; } }
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }
        string ISequenceOfBitBinaryOperation.CSharpNameOfDefaultOperation { get { return CSharpNameOfDefaultOperation; } }

        TypeBase ISequenceOfBitBinaryOperation.ResultType(int objBitCount, int argBitCount) { return ResultType(objBitCount, argBitCount); }

        protected abstract TypeBase ResultType(int objSize, int argSize);

        protected virtual string CSharpNameOfDefaultOperation { get { return Name; } }
        protected virtual bool IsCompareOperator { get { return false; } }
    }

    [Token("+")]
    [Token("-")]
    internal sealed class Sign :
        SequenceOfBitOperation,
        IConverter<IConverter<IPrefixFeature, Sequence>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        IConverter<IPrefixFeature, Sequence> IConverter<IConverter<IPrefixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return new SequenceOperationPrefixFeature(type, this);
        }
        TypeBase ISequenceOfBitPrefixOperation.ResultType(int objBitCount) { return TypeBase.CreateNumber(objBitCount); }
        string ISequenceOfBitOperation.CSharpNameOfDefaultOperation { get { return Name; } }
        string ISequenceOfBitOperation.DataFunctionName { get { return DataFunctionName; } }

        Result ISequenceOfBitOperation.SequenceOperationResult(Category category, Size objSize)
        {
            return Bit.PrefixSequenceOperationResult(category, this, objSize);
        }

        protected override TypeBase ResultType(int objSize, int argSize) { return TypeBase.CreateNumber(BitsConst.PlusSize(objSize, argSize)); }
    }

    [Token("*")]
    internal sealed class Star : SequenceOfBitOperation
    {
        protected override TypeBase ResultType(int objSize, int argSize) { return TypeBase.CreateNumber(BitsConst.MultiplySize(objSize, argSize)); }
    }

    [Token("/")]
    internal sealed class Slash : SequenceOfBitOperation
    {
        protected override TypeBase ResultType(int objSize, int argSize) { return TypeBase.CreateNumber(BitsConst.DivideSize(objSize, argSize)); }
    }
}