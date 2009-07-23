using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation :
        Defineable,
        IConverter<IConverter<IFeature, Sequence>, Bit>,
        IConverter<IFeature, Sequence>,
        ISequenceOfBitBinaryOperation
    {
        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type) { return type.BitOperationFeature(this); }

        IConverter<IFeature, Sequence> IConverter<IConverter<IFeature, Sequence>, Bit>.Convert(Bit type) { return this; }

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
        IConverter<IPrefixFeature, Sequence>,
        ISequenceOfBitPrefixOperation
    {
        IConverter<IPrefixFeature, Sequence> IConverter<IConverter<IPrefixFeature, Sequence>, Bit>.Convert(Bit type) { return this; }
        IPrefixFeature IConverter<IPrefixFeature, Sequence>.Convert(Sequence type) { return type.BitOperationPrefixFeature(this); }
        TypeBase ISequenceOfBitPrefixOperation.ResultType(int objBitCount) { return TypeBase.CreateNumber(objBitCount); }
        string ISequenceOfBitOperation.CSharpNameOfDefaultOperation { get { return Name; } }
        string ISequenceOfBitOperation.DataFunctionName { get { return DataFunctionName; } }
        Result ISequenceOfBitOperation.SequenceOperationResult(Category category, TypeBase typeBase, Size objSize) { return typeBase.PrefixSequenceOperationResult(category, this, objSize); }

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