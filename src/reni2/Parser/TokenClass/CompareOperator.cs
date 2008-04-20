using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class CompareOperator : SequenceOfBitOperation
    {
        internal override bool IsCompareOperator { get { return true; } }

        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateBit;
        }
    }
}