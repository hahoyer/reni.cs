namespace Reni.Parser.TokenClass.Symbol
{
    [Token("<=")]
    class LessEqual : CompareOperator
    {
        internal override string Name { get { return "<="; } }
    }
}