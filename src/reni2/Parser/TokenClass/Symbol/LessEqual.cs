using System;

namespace Reni.Parser.TokenClass.Symbol
{
    [Token("<=")]
    [Serializable]

    sealed class LessEqual : CompareOperator
    {
    }
}