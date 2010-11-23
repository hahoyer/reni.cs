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
                return "StartFunction:\n" + VarDataNewDatacontainer + snippet.Flatten("return {0};\n");
            return VarDataNewDatacontainer + snippet.Flatten("{0}.DropAll();\n");

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

        internal static string DumpPrintText(string text)
        {
            return "DataContainer.DumpPrint(" + text.Quote() + ")";
        }

        internal static string DumpPrint()
        {
            return "DumpPrint()";
        }

        internal static string CreateElse() { return "} else {"; }

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

        internal string CreateLocalBlockEnd(Size size, Size bodySize)
        {
            return CreateMoveBytes(size, Start + bodySize, Start);
        }

        internal string CreateThen(Size condSize) { return "if(" + CreateDataRef(Start, condSize) + "!=0) {"; }

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

        private const string DataName = "data";
        internal const string VarDataNewDatacontainer = "var " + DataName + " = new DataContainer();\n";

        internal static string CreateTopRef() { return DataName; }

        internal static CSharpCodeSnippet List(CodeBase[] data)
        {
            var result = data.Aggregate("", (current, codeBase) => current + Flatten(DataName, codeBase));
            return new CSharpCodeSnippet(result, DataName);
        }

        private static string Flatten(string holder, CodeBase codeBase)
        {
            var resultHeader = holder + ".Expand({0});\n";
            return codeBase
                .CSharpCodeSnippet()
                .Flatten(resultHeader);
        }

        internal static string TopData(Size offset, Size size)
        {
            return "DataContainer.TopData(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")";
        }

        internal static string BitCast(Size size, Size significantSize)
        {
            return "BitCast(" + size.SaveByteCount + ", " + (size - significantSize) + ")";
        }

        internal static string BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize)
        {
            return opToken.DataFunctionName + "(" + size.SaveByteCount + ", " + leftSize.SaveByteCount + ")";
        }

        internal static string BitArrayPrefix(ISequenceOfBitPrefixOperation opToken, Size size)
        {
            return opToken.DataFunctionName + "(" + size.SaveByteCount + ")";
        }
        internal static string Drop(Size size, Size outputSize)
        {
            return "Drop(" + outputSize.SaveByteCount + ")";
        }

        internal static string LocalVariableAccess(string holder, Size offset, Size size)
        {
            return holder + ".DataPart(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")";
        }

        internal static CSharpCodeSnippet LocalVariables(string holderNamePattern, CodeBase[] codeBases)
        {
            var snippets = codeBases.Select(x => x.CSharpCodeSnippet()).ToArray();
            var prerequisites = "";
            for(var i = 0; i < snippets.Length; i++)
                prerequisites += snippets[i].Flatten("var " + String.Format(holderNamePattern, i) + " = {0};\n");
            return new CSharpCodeSnippet(prerequisites, "");
        }

        internal static string LocalVariableReference(string holder, Size offset)
        {
            return holder + ".DataRef(" + offset.SaveByteCount + ")";
        }

        internal static string Call(int functionIndex)
        {
            return "Call(" + Generator.FunctionName(functionIndex) + ")";
        }

        internal static string TopFrame(Size offset, Size size)
        {
            return Generator.FrameArgName + ".DataPartFromBack(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")";
        }

        internal static string Dereference(Size size) { return "Dereference(" + size.SaveByteCount + ")"; }

        internal static CSharpCodeSnippet Fiber(FiberItem[] fiberItems, FiberHead fiberHead)
        {
            var snippet = fiberHead.CSharpCodeSnippet();
            var prerequisites =
                "var " + DataName + fiberHead.ObjectId + " = " + DataName + ";\n"
                + DataName + " = new DataContainer();\n"
                + snippet.Prerequisites
                + "var fiber" + fiberHead.ObjectId + " = " + snippet.Result + ";\n"
                + DataName + " = " + DataName + fiberHead.ObjectId + ";\n";
            var result1 = "fiber" + fiberHead.ObjectId ;

            var fiberItemStrings = fiberItems.Select(x => x.CSharpString()).ToArray();
            var result = fiberItemStrings
                .Aggregate
                ("(" + result1 + ")", (current, s) => current + ("." + s));
            return new CSharpCodeSnippet(prerequisites.Indent(), result);
        }
    }
}