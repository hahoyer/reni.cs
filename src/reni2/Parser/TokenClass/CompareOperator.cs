using System;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("<=")]
    [Token(">=")]
    [Token("<")]
    [Token(">")]
    internal class CompareOperator : SequenceOfBitOperation
    {
        internal override bool IsCompareOperator { get { return true; } }

        internal override TypeBase BitSequenceOperationResultType(int objSize, int argSize)
        {
            return TypeBase.CreateBit;
        }
    }
    [Token("=")]
    internal sealed class Equal : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation { get { return "=="; } }
    }

    [Token("<>")]
    internal sealed class NotEqual : CompareOperator
    {
        internal override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }

}