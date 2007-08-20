using Reni.Context;

namespace Reni.Parser.TokenClass
{
    abstract internal class CompareOperator : Defineable
    {
        /// <summary>
        /// Gets the default sequence operation.
        /// </summary>
        /// <value>The default sequence operation.</value>
        /// created 13.01.2007 19:37
        public override SearchResult SequenceOperation(Type.Base obj)
        {
            return new FoundNumpopResult(this,obj);
        }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">not used.</param>
        /// <param name="argSize">not used.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        public override Type.Base NumericOperationResultType(int objSize, int argSize)
        {
            return Type.Bit.CreateBit;
        }
        /// <summary>
        /// Gets a value indicating whether this instance is logical operator.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is logical operator; otherwise, <c>false</c>.
        /// </value>
        /// created 03.02.2007 15:22
        public override bool IsCompareOperator { get { return true; } }
    }
}