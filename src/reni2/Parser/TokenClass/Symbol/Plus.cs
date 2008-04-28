using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class Plus : SequenceOfBitOperation
    {
        internal override string Name { get { return "+"; } }
        internal override string CSharpNameOfDefaultOperation { get { return "+"; } }

        internal protected override bool IsBitSequencePrefixOperation { get { return true; } }

        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateNumber(BitsConst.PlusSize(objSize, argSize));
        }
    }
}