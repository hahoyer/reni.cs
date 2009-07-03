using System;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation : Defineable,
        IConverter<IConverter<IFeature, Sequence>, Bit>,
        IConverter<IFeature, Sequence>,
        IConverter<IPrefixFeature, Sequence>
    {
        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type) { return type.BitOperationFeature(this); }

        IConverter<IFeature, Sequence> IConverter<IConverter<IFeature, Sequence>, Bit>.Convert(Bit type) { return this; }

        IPrefixFeature IConverter<IPrefixFeature, Sequence>.Convert(Sequence type) { return type.BitOperationFeature(this); }

        internal override TypeBase BitSequenceOperationResultType(int objSize)
        {
            return TypeBase.CreateNumber(objSize);
        }
    }

    internal class PrefixableSequenceOfBitOperation : SequenceOfBitOperation, IConverter<IConverter<IPrefixFeature, Sequence>, Bit>
    {
        IConverter<IPrefixFeature, Sequence> IConverter<IConverter<IPrefixFeature, Sequence>, Bit>.Convert(Bit type) { return this; }
    }


}