namespace Reni.Parser.TokenClass.Symbol
{
    internal class Minus : Defineable
    {
        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal override Type.Base BitSequenceOperationResultType(int objSize, int argSize)
        {
            return Type.Base.CreateNumber(BitsConst.PlusSize(objSize, argSize));
        }

        internal override bool IsBitSequenceOperation { get { return true; } }

        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        internal override string CSharpNameOfDefaultOperation { get { return "-"; } }

        /// <summary>
        /// Gets the numeric prefix operation.
        /// </summary>
        /// <value>The numeric prefix operation.</value>
        /// created 02.02.2007 23:03
        internal override bool IsBitSequencePrefixOperation { get { return true; } }
    }
}