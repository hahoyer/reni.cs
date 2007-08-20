using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Symbol
{
    class Minus: Defineable
    {
        private FoundNumericPrefixOperationResult _foundNumericPrefixOperationResult;

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">Size of the obj.</param>
        /// <param name="argSize">Size of the arg.</param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        public override Type.Base NumericOperationResultType(int objSize, int argSize)
        {
            return Type.Base.CreateNumber(BitsConst.PlusSize(objSize, argSize));
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// created 15.09.2006 23:34
        public Minus()
        {
            _foundNumericPrefixOperationResult = new FoundNumericPrefixOperationResult(this);
        }

        public override SearchResult SequenceOperation(Type.Base obj)
        {
            return new FoundNumpopResult(this,obj);
        }

        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        public override string CSharpNameOfDefaultOperation { get { return "-"; } }

        /// <summary>
        /// Gets the numeric prefix operation.
        /// </summary>
        /// <value>The numeric prefix operation.</value>
        /// created 02.02.2007 23:03
        [DumpData(false)]
        internal override PrefixSearchResult NumericPrefixOperation { get { return _foundNumericPrefixOperationResult; } }
    }

}
