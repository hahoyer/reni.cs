namespace Reni.Parser.TokenClass.Symbol
{
    [Token("<>")]
    internal sealed class LessGreater : CompareOperator
    {
        internal override string Name { get { return "<>"; } }
        internal override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }
}