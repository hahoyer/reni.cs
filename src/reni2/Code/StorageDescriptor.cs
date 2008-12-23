using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    internal class StorageDescriptor : ReniObject
    {
        private Size _start;
        private readonly Size _dataEndAddr;
        private readonly Size _frameSize;

        public StorageDescriptor(Size dataEndAddr, Size frameSize)
        {
            if(dataEndAddr == null)
                throw new ArgumentNullException("dataEndAddr");
            if(frameSize == null)
                throw new ArgumentNullException("frameSize");

            _dataEndAddr = dataEndAddr;
            _frameSize = frameSize;
            _start = dataEndAddr;
        }

        public Size Start { get { return _start; } }
        public Size DataEndAddr { get { return _dataEndAddr; } }
        public void ShiftStartAddress(Size deltaSize) { _start += deltaSize; }

        private string CreateFunctionReturn()
        {
            var resultSize = DataEndAddr - Start;
            if(_frameSize.IsZero && resultSize.IsZero)
                return "";
            return CreateMoveBytesToFrame(resultSize, resultSize, Start)
                   + "; // StorageDescriptor.CreateFunctionReturn";
        }

        internal string CreateFunctionBody(List<LeafElement> data, bool isFunction)
        {
            var result = GetStatements(data);
            if(isFunction)
                result =
                    "StartFunction:\n"
                        + result
                            + CreateFunctionReturn();
            return result;
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
            throw new NotImplementedException();
        }

        internal string CreateBitArrayPrefixOp(Defineable opToken, Size size, Size argSize)
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

        internal string CreateBitArrayOp(Defineable opToken, Size resultSize, Size leftSize, Size rightSize)
        {
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

        internal string CreateBitArrayOpThen(Defineable opToken, Size leftSize, Size rightSize, int thenElseObjectId, Size condSize)
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
                                                    + ") {" ;
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
            throw new NotImplementedException();
        }

        internal string CreateBitsArray(Size size, BitsConst data)
        {
            if(IsBuildInIntType(size))
                return
                    CreateDataRef(Start - size, size)
                        + " = "
                            + CreateIntCast(size)
                                + "("
                                    + data.CodeDump()
                                        + ")";

            var result = "";
            for(var i = 0; i < size.ByteCount; i++)
            {
                result += ", ";
                result += data.Byte(i);
            }

            return "Data.BitsArray("
                + size.ByteCount
                    + ", "
                        + CreateDataPtr(Start - size)
                            + result
                                + ")";
        }

        internal string CreateCall(int index, Size frameSize) { return Generator.FunctionMethodName(index) + "(" + CreateDataPtr(Start + frameSize) + ")"; }

        internal string CreateDumpPrint(Size leftSize, Size rightSize)
        {
            if(rightSize.IsZero)
                return CreateDumpPrintOperation(leftSize);
            NotImplementedMethod(leftSize, rightSize);
            throw new NotImplementedException();
        }

        internal static string CreateDumpPrintText(string text) { return "Data.DumpPrint(" + text.Quote() + ")"; }

        internal static string CreateElse(int objectId) { return "} else {" ; }

        internal static string CreateEndCondional(int objectId) { return "}"; }

        internal static string CreateRecursiveCall() { return "goto StartFunction"; }

        internal string CreateRefPlus(Size size, int right)
        {
            if(right == 0)
                return "";

            if(size.ToInt() == 32)
                return CreateDataRef(Start, size) + " += " + right;

            NotImplementedFunction(this, size, right);
            throw new NotImplementedException();
        }

        internal string CreateTopFrame(RefAlignParam refAlignParam, Size offset, Size targetSize)
        {
            if(refAlignParam.Is_3_32)
                return CreateMoveBytesFromFrame(targetSize, Start - targetSize, offset*-1);

            NotImplementedFunction(this, refAlignParam, offset, targetSize, targetSize);
            throw new NotImplementedException();
        }

        internal string CreateStatementEnd(Size size, Size bodySize) { return CreateMoveBytes(size, Start + bodySize, Start); }

        internal string CreateThen(int objectId, Size condSize) { return "if(" + CreateDataRef(Start, condSize) + "!=0) {"; }

        internal string CreateTopData(RefAlignParam refAlignParam, Size offset, Size targetSize)
        {
            if(refAlignParam.Is_3_32)
                return CreateMoveBytes(targetSize, Start - targetSize, Start + offset);

            NotImplementedFunction(this, refAlignParam, offset, targetSize);
            throw new NotImplementedException();
        }

        internal string CreateTopRef(RefAlignParam refAlignParam, Size offset)
        {
            if(refAlignParam.Is_3_32)
                return CreateDataRef(Start - refAlignParam.RefSize, refAlignParam.RefSize)
                    + " = "
                        + CreateIntCast(refAlignParam.RefSize)
                            + CreateDataPtr(Start + offset);

            NotImplementedMethod(refAlignParam);
            throw new NotImplementedException();
        }

        internal string CreateFrameRef(RefAlignParam refAlignParam, Size offset)
        {
            if(refAlignParam.Is_3_32)
                return CreateDataRef(Start - refAlignParam.RefSize, refAlignParam.RefSize)
                    + " = "
                        + CreateIntCast(refAlignParam.RefSize)
                            + CreateFrameBackPtr(offset*-1);

            NotImplementedMethod(refAlignParam, offset);
            throw new NotImplementedException();
        }

        internal string CreateUnref(RefAlignParam refAlignParam, Size targetSize)
        {
            if(refAlignParam.Is_3_32)
            {
                if(IsBuildInIntType(targetSize))
                    return CreateDataRef(Start + refAlignParam.RefSize - targetSize, targetSize)
                        + " = "
                            + CreateCastToIntRef(targetSize, CreateDataRef(Start, refAlignParam.RefSize));
                return "Data.MoveBytes("
                    + targetSize.ByteCount
                        + ", "
                            + CreateDataPtr(Start + refAlignParam.RefSize - targetSize)
                                + ", "
                                    + CreateCastToIntPtr(Size.Byte, CreateDataRef(Start, refAlignParam.RefSize))
                                        + ")";
            }
            NotImplementedMethod(refAlignParam, targetSize);
            throw new NotImplementedException();
        }

        internal string CreateDumpPrintOperation(Size size)
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

        private string CreateBinaryOperation(Size leftSize, Defineable opToken, Size rightSize)
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        private string GetStatements(IList<LeafElement> data)
        {
            var statements = "";
            for(var i = 0; i < data.Count; i++)
            {
                statements += data[i].Statements(this);
                ShiftStartAddress(data[i].DeltaSize);
            }
            return statements;
        }

    }
}