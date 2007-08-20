namespace Reni.Parser.TokenClass.Symbol
{
    class Less : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation
        {
            get { return "<"; }
        }
    }
}