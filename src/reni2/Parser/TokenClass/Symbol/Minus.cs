using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    [Token("-")]
    internal class Minus : SequenceOfBitOperation
    {
        internal protected override bool IsBitSequencePrefixOperation { get { return true; } }

        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateNumber(BitsConst.PlusSize(objSize, argSize));
        }
    }
}