namespace Reni.Parser.TokenClass.Symbol
{
    [Token("=")]
    internal sealed class Equal : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation { get { return "=="; } }
        internal override string Name { get { return "="; } }
    }
}
