namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class Equal : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation { get { return "=="; } }
    }
}
