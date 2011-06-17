using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Type
{
    internal interface ISequenceOfBitPrefixOperation
    {
        [DisableDump]
        string CSharpNameOfDefaultOperation { get; }

        [DisableDump]
        string DataFunctionName { get; }

        Result SequenceOperationResult(Category category, Size objSize);
    }
}