using Reni.Basics;
using Reni.Struct;
using System.Reflection;

namespace Reni.Code;

interface IVisitor
{
    void ArrayGetter(Size elementSize, Size indexSize);
    void ArraySetter(Size elementSize, Size indexSize);
    void Assign(Size targetSize);
    void BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize);
    void BitArrayPrefixOp(string operation, Size size, Size argSize);
    void BitCast(Size size, Size targetSize, Size significantSize);
    void BitsArray(Size size, BitsConst data);
    void Call(Size outputSize, FunctionId functionId, Size argsAndRefsSize);
    void DePointer(Size size, Size dataSize);
    void Drop(Size beforeSize, Size afterSize);
    void Fiber(FiberHead fiberHead, FiberItem[] fiberItems);
    void ForeignCall(Size outputSize, MethodInfo method, Size inputSize);
    void List(CodeBase[] data);
    void PrintNumber(Size leftSize, Size rightSize);
    void PrintText(Size leftSize, Size itemSize);
    void PrintText(string dumpPrintText);
    void RecursiveCall();
    void RecursiveCallCandidate();
    void ReferencePlus(Size right);
    void ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode);
    void TopData(Size offset, Size size, Size dataSize);
    void TopFrameData(Size offset, Size size, Size dataSize);
    void TopRef(Size offset);
    void TopFrameRef(Size offset);
}