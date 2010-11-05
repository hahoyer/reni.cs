using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    internal sealed class CSharpGenerator : ReniObject
    {
        private Size _start;
        private readonly Size _dataEndAddr;
        private readonly Size _frameSize;

        public CSharpGenerator(Size dataEndAddr, Size frameSize)
        {
            if(dataEndAddr == null)
                throw new ArgumentNullException("dataEndAddr");
            if(frameSize == null)
                throw new ArgumentNullException("frameSize");

            _dataEndAddr = dataEndAddr;
            _frameSize = frameSize;
            _start = dataEndAddr;
        }

        private Size Start { get { return _start; } }
        private Size DataEndAddr { get { return _dataEndAddr; } }
        private void ShiftStartAddress(Size deltaSize) { _start += deltaSize; }

        private string CreateFunctionReturn()
        {
            var resultSize = DataEndAddr - Start;
            if(_frameSize.IsZero && resultSize.IsZero)
                return "";
            return CreateMoveBytesToFrame(resultSize, resultSize, Start)
                   + "; // FunctionReturn";
        }

        internal string CreateFunctionBody(CodeBase data, bool isFunction)
        {
            var snippet = data.CSharpCodeSnippet();
            if(isFunction)
            {
                return "StartFunction:\n"
                       + snippet.Prerequisites + "\n"
                       + "return " + snippet.Result + "\n";
            }
            return snippet.Prerequisites 
                + "\n"
                + snippet.Result 
                + "\n";

        }

        internal string CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            if(refAlignParam.Is_3_32)
            {
                var dest = CreateDataRef(Start + refAlignParam.RefSize, refAlignParam.RefSize);
                var source = CreateDataRef(Start, refAlignParam.RefSize);
                if(IsBuildInIntType(size))
                    return "*("
                        + CreateIntPtrCast(size)
                            + dest
                                + ") = *("
                                    + CreateIntPtrCast(size) 
                                    + source 
                                    + ")";

                return "Data.MoveBytes("
                    + size.ByteCount
                        + ", "
                            + "(sbyte*)" + dest
                                + ", "
                                    + "(sbyte*)" + source
                                        + ")";
            }

            NotImplementedFunction(this, refAlignParam, size);
            return null;
        }

        internal string CreateBitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize)
        {
            if(IsBuildInIntType(size))
                return CreateDataRef(Start, size)
                    + " = "
                        + CreateIntCast(size)
                            + "("
                                + opToken.CSharpNameOfDefaultOperation
                                    + " "
                                        + CreateDataRef(Start, argSize)
                                            + ")";

            return "Data."
                + opToken.DataFunctionName
                    + "("
                        + size.ByteCount
                            + ", "
                                + CreateDataPtr(Start)
                                    + ", "
                                        + argSize.ByteCount
                                            + ")";
        }

        internal string CreateBitArrayOp(ISequenceOfBitBinaryOperation opToken, Size resultSize, Size leftSize, Size rightSize)
        {
            Tracer.Assert(opToken.CSharpNameOfDefaultOperation != "");

            if(IsBuildInIntType(leftSize) && IsBuildInIntType(rightSize))
            {
                var value = CreateBinaryOperation(leftSize, opToken, rightSize);
                if(opToken.IsCompareOperator)
                    value = "(" + value + "? -1:0" + ")";
                return CreateStoreResult(resultSize, leftSize + rightSize) + value;
            }

            return "Data."
                + opToken.DataFunctionName
                    + "("
                        + resultSize.ByteCount
                            + ", "
                                + CreateDataPtr(Start + leftSize + rightSize - resultSize)
                                    + ", "
                                        + leftSize.ByteCount
                                            + ", "
                                                + CreateDataPtr(Start + rightSize)
                                                    + ", "
                                                        + rightSize.ByteCount
                                                            + ", "
                                                                + CreateDataPtr(Start)
                                                                    + ")";
        }

        internal string CreateBitArrayOpThen(ISequenceOfBitBinaryOperation opToken, Size leftSize, Size rightSize)
        {
            if(IsBuildInIntType(leftSize) && IsBuildInIntType(rightSize))
                return "if("
                    + CreateDataRef(Start + rightSize, leftSize)
                        + " "
                            + opToken.CSharpNameOfDefaultOperation
                                + " "
                                    + CreateDataRef(Start, rightSize)
                                        + ") {";
            return "if(Data."
                + opToken.DataFunctionName
                + "("
                  + leftSize.ByteCount
                  + ", "
                  + CreateDataPtr(Start + rightSize)
                  + ", "
                  + rightSize.ByteCount
                  + ", "
                  + CreateDataPtr(Start)
                + ")) {" ;
        }

        internal string CreateBitCast(Size targetSize, Size size, Size significantSize)
        {
            var targetBytes = targetSize.ByteCount;
            var bytes = size.ByteCount;
            if(targetBytes == bytes)
            {
                if(significantSize == Size.Byte*bytes)
                    return "";
                var bits = (size - significantSize).ToInt();
                var oldStart = Start;
                if(IsBuildInIntType(size))
                {
                    var result =
                        CreateDataRef(oldStart, size)
                            + " = "
                                + CreateIntCast(size)
                                    + "("
                                        + CreateIntCast(size)
                                            + "("
                                                + CreateDataRef(oldStart, size) + " << " + bits
                                                    + ") >> "
                                                        + bits
                                                            + ")";
                    return result;
                }
                return "Data.BitCast("
                    + size.ByteCount
                        + ", "
                            + CreateDataPtr(oldStart)
                                + ", "
                                    + bits
                                        + ")";
            }

            NotImplementedFunction(this, targetSize, size);
            return null;
        }

        internal static string CreateBitArray(Size size, BitsConst data)
        {
            var result = "new DataContainer(";
            for(var i = 0; i < size.ByteCount; i++)
            {
                if(i > 0)
                    result += ", ";
                result += data.Byte(i);
            }
            result += ")";
            return result;
        }

        internal string CreateCall(int index, Size frameSize) { return Generator.FunctionName(index) + "(" + CreateDataPtr(Start + frameSize) + ")"; }

        internal string CreateDumpPrint(Size leftSize, Size rightSize)
        {
            if(rightSize.IsZero)
                return CreateDumpPrintOperation(leftSize);
            NotImplementedMethod(leftSize, rightSize);
            return null;
        }

        internal static string CreateDumpPrintText(string text)
        {
            return "Data.DumpPrint(" + text.Quote() + ")";
        }

        internal static string CreateElse() { return "} else {" ; }

        internal static string CreateEndCondional() { return "}"; }

        internal static string CreateRecursiveCall() { return "goto StartFunction"; }

        internal string CreateRefPlus(Size size, int right)
        {
            if(right == 0)
                return "";

            if(size.ToInt() == 32)
                return CreateDataRef(Start, size) + " += " + right;

            NotImplementedFunction(this, size, right);
            return null;
        }

        internal static string CreateTopFrame(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
        {
            NotImplementedFunction(null, refAlignParam, offset, size, dataSize);
            return null;
        }

        internal string CreateLocalBlockEnd(Size size, Size bodySize)
        {
            return CreateMoveBytes(size, Start + bodySize, Start);
        }

        internal string CreateThen(Size condSize) { return "if(" + CreateDataRef(Start, condSize) + "!=0) {"; }

        internal static string CreateTopRef(RefAlignParam refAlignParam, Size offset)
        {
            NotImplementedFunction(refAlignParam,offset);
            return null;
        }

        internal static string CreateFrameRef(RefAlignParam refAlignParam, Size offset)
        {
            NotImplementedFunction(null, refAlignParam, offset);
            return null;
        }

        internal string CreateUnref(RefAlignParam refAlignParam, Size size, Size dataSize)
        {
            if(refAlignParam.Is_3_32)
            {
                if(IsBuildInIntType(size))
                    return CreateDataRef(Start + refAlignParam.RefSize - size, size)
                        + " = "
                            + CreateCastToIntRef(dataSize, CreateDataRef(Start, refAlignParam.RefSize));
                return "Data.MoveBytes("
                    + dataSize.ByteCount
                        + ", "
                            + CreateDataPtr(Start + refAlignParam.RefSize - size)
                                + ", "
                                    + CreateCastToIntPtr(Size.Byte, CreateDataRef(Start, refAlignParam.RefSize))
                                        + ")";
            }
            NotImplementedMethod(refAlignParam, size, dataSize);
            return null;
        }

        private string CreateDumpPrintOperation(Size size)
        {
            if(IsBuildInIntType(size))
                return "Data.DumpPrint(" + CreateDataRef(Start, size) + ")";
            return "Data.DumpPrint(" + size.ByteCount + ", " + CreateDataPtr(Start) + ")";
        }

        private string CreateStoreResult(Size resultSize, Size argsSize)
        {
            return CreateDataRef(Start + argsSize - resultSize, resultSize)
                + " = "
                    + CreateIntCast(resultSize);
        }

        private string CreateBinaryOperation(Size leftSize, ISequenceOfBitBinaryOperation opToken, Size rightSize)
        {
            return "("
                + CreateDataRef(Start + rightSize, leftSize)
                    + " "
                        + opToken.CSharpNameOfDefaultOperation
                            + " "
                                + CreateDataRef(Start, rightSize)
                                    + ")";
        }

        private static string CreateCastToIntPtr(Size size, string result) { return "(" + CreateIntPtrCast(size) + " " + result + ")"; }

        private static string CreateCastToIntRef(Size size, string result) { return "(*" + CreateIntPtrCast(size) + " " + result + ")"; }

        private static string CreateDataPtr(Size start) { return "(data+" + start.SaveByteCount + ")"; }

        private static string CreateDataRef(Size start, Size size) { return CreateCastToIntRef(size, CreateDataPtr(start)); }

        private static string CreateFrameBackPtr(Size start) { return "(frame-" + start.SaveByteCount + ")"; }

        private static string CreateFrameBackRef(Size start, Size size) { return CreateCastToIntRef(size, CreateFrameBackPtr(start)); }

        private static string CreateIntCast(Size size)
        {
            var bits = size.ByteCount*8;
            switch(bits)
            {
                case 8:
                case 16:
                case 32:
                case 64:
                    return "(" + CreateIntType(size) + ")";
            }
            NotImplementedFunction(size, "bits", bits);
            return null;
        }

        private static string CreateIntPtrCast(Size size) { return "(" + CreateIntType(size) + "*)"; }

        private static string CreateIntType(Size size)
        {
            var bits = size.ByteCount*8;
            switch(bits)
            {
                case 8:
                    return "sbyte";
                case 16:
                case 32:
                case 64:
                    return "Int" + bits;
            }
            NotImplementedFunction(size, "bits", bits);
            return null;
        }

        private static string CreateMoveBytes(Size size, Size destStart, Size sourceStart)
        {
            if(IsBuildInIntType(size))
                return CreateDataRef(destStart, size)
                    + " = "
                        + CreateDataRef(sourceStart, size)
                    ;
            return "Data.MoveBytes("
                + size.ByteCount
                    + ", "
                        + CreateDataPtr(destStart)
                            + ", "
                                + CreateDataPtr(sourceStart)
                                    + ")";
        }

        private static string CreateMoveBytesFromFrame(Size size, Size destStart, Size sourceStart)
        {
            if(size.IsZero)
                return "";

            if(IsBuildInIntType(size))
                return CreateDataRef(destStart, size)
                    + " = "
                        + CreateFrameBackRef(sourceStart, size)
                    ;
            return "Data.MoveBytes("
                + size.ByteCount
                    + ", "
                        + CreateDataPtr(destStart)
                            + ", "
                                + CreateFrameBackPtr(sourceStart)
                                    + ")";
        }

        private static string CreateMoveBytesToFrame(Size size, Size destStart, Size sourceStart)
        {
            if(size.IsZero)
                return "";

            if(IsBuildInIntType(size))
                return CreateFrameBackRef(destStart, size)
                    + " = "
                        + CreateDataRef(sourceStart, size)
                    ;
            return "Data.MoveBytes("
                + size.ByteCount
                    + ", "
                        + CreateFrameBackPtr(destStart)
                            + ", "
                                + CreateDataPtr(sourceStart)
                                    + ")";
        }

        private static bool IsBuildInIntType(Size size)
        {
            var bits = size.ByteCount*8;
            switch(bits)
            {
                case 8:
                case 16:
                case 32:
                case 64:
                    return true;
            }
            return false;
        }

        private static int _nextListId = 0;

        internal static CSharpCodeSnippet CreateList(CodeBase[] data)
        {
            var holder = "list" + _nextListId++;
            var result = holder + " = new DataContainer();\n";
            foreach(var codeBase in data)
            {
                var snippet = codeBase.CSharpCodeSnippet();
                if(snippet.Prerequisites != "")
                    result += snippet.Prerequisites + "\n";
                result += holder + ".Expand(" + snippet.Result + ");\n";
            }
            return new CSharpCodeSnippet(result, holder);
        }

        internal static string TopData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
        {
            return "DataContainer.TopData(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")";
        }

        internal static string BitCast(Size size, Size significantSize)
        {
            return "BitCast(" + size.SaveByteCount + ", " + significantSize + ")";
        }

        public static string BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            return opToken.DataFunctionName + "(" + size.SaveByteCount + ", " + leftSize.SaveByteCount + ")";
        }

        public static string Drop(Size size, Size outputSize) { return "Drop(" + outputSize.SaveByteCount + ")"; }
    }

    internal sealed class CSharpCodeSnippet
    {
        internal readonly string Prerequisites;
        internal readonly string Result;

        internal CSharpCodeSnippet(string prerequisites, string result)
        {
            Prerequisites = prerequisites;
            Result = result;
        }
    }
}