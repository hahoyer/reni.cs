using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code
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