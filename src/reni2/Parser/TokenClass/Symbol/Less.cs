namespace Reni.Parser.TokenClass.Symbol
{
    class Less : CompareOperator
    {
        internal override string Name { get { return "<"; } }
        internal override string CSharpNameOfDefaultOperation
        {
            get { return "<"; }
        }
    }
}