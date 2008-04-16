using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class Star : SequenceOfBitOperation
    {

        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        internal override string CSharpNameOfDefaultOperation { get { return "*"; } }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal override Type.Base BitSequenceOperationResultType(int objSize, int argSize)
        {
            return Type.Base.CreateNumber(BitsConst.MultiplySize(objSize, argSize));
        }
    }
}
