namespace Reni.Parser.TokenClass.Symbol
{
    internal sealed class LessGreater : CompareOperator
    {
        internal override string Name { get { return "<>"; } }
        internal override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }
}