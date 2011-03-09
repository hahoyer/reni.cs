using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Sequence;

namespace Reni.ReniParser.TokenClasses
{
    internal class CompareOperator : SequenceOfBitOperation
    {
        protected override bool IsCompareOperator { get { return true; } }
        protected override int ResultSize(int objSize, int argSize) { return 1; }
    }

    internal sealed class Equal : CompareOperator
    {
        protected override string CSharpNameOfDefaultOperation { get { return "=="; } }
    }

    internal sealed class NotEqual : CompareOperator
    {
        protected override string CSharpNameOfDefaultOperation { get { return "!="; } }
    }
}