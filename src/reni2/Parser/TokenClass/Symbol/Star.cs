using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class Star : SequenceOfBitOperation
    {
        internal override string Name { get { return "*"; } }
        internal override string CSharpNameOfDefaultOperation { get { return "*"; } }

        internal override Type.TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return Type.TypeBase.CreateNumber(BitsConst.MultiplySize(objSize, argSize));
        }
    }
}
