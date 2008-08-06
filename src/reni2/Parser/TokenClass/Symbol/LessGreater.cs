using System;

namespace Reni.Parser.TokenClass.Symbol
{
    [Token("<>")]
    [Serializable]
    internal sealed class LessGreater : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }
}