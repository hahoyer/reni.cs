using Reni.Context;

namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class Equal : CompareOperator
    {
        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        public override string CSharpNameOfDefaultOperation { get { return "=="; } }
    }
}
