using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Code
{
    internal interface IVisitor
    {
        void Assign(Size targetSize);
        void BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize);
        void BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize);
        void BitCast(Size size, Size targetSize, Size significantSize);
        void BitsArray(Size size, BitsConst data);
        void Call(Size size, int functionIndex, Size argsAndRefsSize);
        void Dereference(Size size, Size dataSize);
        void Drop(Size beforeSize, Size afterSize);
        void PrintNumber(Size leftSize, Size rightSize);
        void PrintText(Size leftSize, Size itemSize);
        void PrintText(string dumpPrintText);
        void Fiber(FiberHead fiberHead, FiberItem[] fiberItems);
        void List(CodeBase[] data);
        void LocalBlockEnd(Size size, Size intermediateSize);
        void LocalVariableAccess(string holder, Size offset, Size size, Size dataSize);
        void LocalVariableDefinition(string holderName, Size valueSize);
        void LocalVariableReference(string holder, Size offset);
        void RecursiveCall();
        void ReferenceCode(IReferenceInCode context);
        void RefPlus(Size right);
        void ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode);
        void TopData(Size offset, Size size, Size dataSize);
        void TopFrameData(Size offset, Size size, Size dataSize);
        void TopRef(Size offset);
        void TopFrameRef(Size offset);
    }
}