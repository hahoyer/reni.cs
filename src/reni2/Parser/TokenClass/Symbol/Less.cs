namespace Reni.Parser.TokenClass.Symbol
{
    [Token("<")]
    class Less : CompareOperator
    {
        internal override string Name { get { return "<"; } }
    }
}