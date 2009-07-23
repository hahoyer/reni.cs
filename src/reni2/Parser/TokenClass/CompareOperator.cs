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
        protected override bool IsCompareOperator { get { return true; } }

        protected override TypeBase ResultType(int objSize, int argSize)
        {
            return TypeBase.CreateBit;
        }
    }
    [Token("=")]
    internal sealed class Equal : CompareOperator
    {
        protected override string CSharpNameOfDefaultOperation { get { return "=="; } }
    }

    [Token("<>")]
    internal sealed class NotEqual : CompareOperator
    {
        protected override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }

}