using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Type
{
    internal interface ISequenceOfBitBinaryOperation
    {
        int ResultSize(int objBitCount, int argBitCount);
        bool IsCompareOperator { get; }
        string DataFunctionName { get; }
        string CSharpNameOfDefaultOperation { get; }
    }
}