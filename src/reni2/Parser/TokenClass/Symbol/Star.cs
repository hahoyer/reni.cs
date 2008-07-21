using System;
using HWClassLibrary.Debug;
using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    [Token("*")]
    internal sealed class Star : SequenceOfBitOperation
    {
        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateNumber(BitsConst.MultiplySize(objSize, argSize));
        }
    }
}