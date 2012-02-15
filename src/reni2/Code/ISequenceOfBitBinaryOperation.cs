using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Code
{
    internal interface ISequenceOfBitBinaryOperation
    {
        int ResultSize(int objBitCount, int argBitCount);
        string DataFunctionName { get; }
    }
}