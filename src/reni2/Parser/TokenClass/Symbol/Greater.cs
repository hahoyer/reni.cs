namespace Reni.Parser.TokenClass.Symbol
{
    [Token(">")]
    class Greater : CompareOperator
    {
        internal override string Name { get { return ">"; } }
        internal override string CSharpNameOfDefaultOperation
        {
            get { return ">"; }
        }
    }
}