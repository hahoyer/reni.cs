namespace Reni.Parser.TokenClass.Symbol
{
    class Greater: CompareOperator
    {
        internal override string Name { get { return ">"; } }
        internal override string CSharpNameOfDefaultOperation
        {
            get { return ">"; }
        }
    }
}