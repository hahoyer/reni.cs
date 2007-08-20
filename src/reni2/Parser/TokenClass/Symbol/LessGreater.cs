namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class LessGreater : CompareOperator
    {
        /// <summary>
        /// Gets the name of token for C# generation.
        /// </summary>
        /// <value>The name of the C sharp.</value>
        /// created 08.01.2007 15:02
        internal override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }
}