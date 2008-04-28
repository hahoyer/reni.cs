namespace Reni.Parser.TokenClass.Symbol
{
    class GreaterEqual : CompareOperator
    {
        internal override string Name { get { return ">="; } }
        internal override string CSharpNameOfDefaultOperation
        {
            get { return ">="; }
        }
    }
}