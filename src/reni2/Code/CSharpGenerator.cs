using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    internal sealed class CSharpGenerator : ReniObject
    {
        private readonly Size _start;

        public CSharpGenerator(Size dataEndAddr)
        {
            _start = dataEndAddr;
        }

        private Size Start { get { return _start; } }

        internal static string CreateFunctionBody(CodeBase data, bool isFunction)
        {
            var snippet = data.CSharpCodeSnippet();
            if(isFunction)
                return "StartFunction:\n" + snippet.Flatten("return {0};\n");
            return snippet.Flatten("{0}.Drop();\n");

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
            var result = data.Aggregate("var "+ holder + " = new DataContainer();\n", (current, codeBase) => current + codeBase.CSharpCodeSnippet().Flatten(holder + ".Expand({0});\n"));
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

        internal static string BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            return opToken.DataFunctionName + "(" + size.SaveByteCount + ", " + leftSize.SaveByteCount + ")";
        }

        internal static string Drop(Size size, Size outputSize)
        {
            return "Drop(" + outputSize.SaveByteCount + ")";
        }

        internal static string LocalVariableAccess(string holder, Size size, Size dataSize)
        {
            return holder + ".DataPart(" + size.SaveByteCount + ")";
        }

        internal static CSharpCodeSnippet LocalVariables(string holderNamePattern, CodeBase[] codeBases)
        {
            var snippets = codeBases.Select(x => x.CSharpCodeSnippet()).ToArray();
            var prerequisites = "";
            for(var i = 0; i < snippets.Length; i++)
                prerequisites += snippets[i].Flatten("var " + String.Format(holderNamePattern, i) + " = {0};\n");
            return new CSharpCodeSnippet(prerequisites, "");
        }
    }
}