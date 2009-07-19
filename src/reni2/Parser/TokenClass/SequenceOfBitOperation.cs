using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation : Defineable,
                                                     IConverter<IConverter<IFeature, Sequence>, Bit>,
                                                     IConverter<IFeature, Sequence>,
                                                     IConverter<IPrefixFeature, Sequence>
    {
        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type)
        {
            return type.BitOperationFeature(this);
        }

        IConverter<IFeature, Sequence> IConverter<IConverter<IFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return this;
        }

        IPrefixFeature IConverter<IPrefixFeature, Sequence>.Convert(Sequence type)
        {
            return type.BitOperationFeature(this);
        }

        internal override TypeBase BitSequenceOperationResultType(int objSize)
        {
            return TypeBase.CreateNumber(objSize);
        }
    }

    [Token("+")]
    [Token("-")]
    internal sealed class Sign : SequenceOfBitOperation, IConverter<IConverter<IPrefixFeature, Sequence>, Bit>
    {
        IConverter<IPrefixFeature, Sequence> IConverter<IConverter<IPrefixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return this;
        }

        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateNumber(BitsConst.PlusSize(objSize, argSize));
        }
    }

    [Token("*")]
    internal sealed class Star : SequenceOfBitOperation
    {
        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize) { return TypeBase.CreateNumber(BitsConst.MultiplySize(objSize, argSize)); }
    }

    [Token("/")]
    internal sealed class Slash : SequenceOfBitOperation
    {
        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize) { return TypeBase.CreateNumber(BitsConst.DivideSize(objSize, argSize)); }
    }

}