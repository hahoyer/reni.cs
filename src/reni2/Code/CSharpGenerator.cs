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
        private readonly Size _start;

        public CSharpGenerator(Size dataEndAddr) { _start = dataEndAddr; }

        private Size Start { get { return _start; } }

        internal static string CreateFunctionBody(CodeBase data, bool isFunction)
        {
            var maxSize = data.MaxSize;
            var head = string.Format("var data = new byte[{0}];\n", maxSize.SaveByteCount);
            var body = data.ReversePolish(maxSize);
            return head + body;

            //var maxSize = data.MaxSize;
            //var snippet = data.CSharpCodeSnippet();
            //if(isFunction)
            //    return "StartFunction:\n" + VarDataNewDatacontainer + snippet.Flatten("return {0};\n");
            //return VarDataNewDatacontainer + snippet.Flatten("{0}.DropAll();\n");
        }

        internal static string List(Size top, int objectId, CodeBase[] data)
        {
            var result = "";
            foreach(var codeBase in data)
            {
                result += "/* " + codeBase.NodeDump + " */\n";
                result += codeBase.ReversePolish(top);
                top -= codeBase.Size;
            }
            return Indented("List", objectId, result);
        }

        private static string Indented(string tag, int objectId, string result) { return "//" + tag + " " + objectId + ("\n" + result).Indent() + "\n"; }

        internal static string LocalVariables(Size top, int objectId, string holderNamePattern, CodeBase[] codeBases)
        {
            var result = "";
            for(var i = 0; i < codeBases.Length; i++)
            {
                var codeBase = codeBases[i];
                result += "/* " + codeBase.NodeDump + " */\n";
                result += codeBase.ReversePolish(top);
                result += LocalVariable(top - codeBase.Size, String.Format(holderNamePattern, i), codeBase.Size);
            }
            return Indented("LocalVariables", objectId, result);
        }

        internal static string Fiber(Size top, int objectId, FiberItem[] fiberItems, FiberHead fiberHead)
        {
            var result = "";
            result += "/* " + fiberHead.NodeDump + " */\n";
            result += fiberHead.ReversePolish(top);
            top -= fiberHead.Size;
            foreach(var fiberItem in fiberItems)
            {
                result += "/* " + fiberItem.NodeDump + " */\n";
                result += fiberItem.ReversePolish(top);
                top -= fiberItem.DeltaSize;
            }
            return Indented("Fiber", objectId, result);
        }

        private static string LocalVariable(Size top, string name, Size size) { return string.Format("var {0} = Data.Get(data, {2}, {1});\n", name, size.SaveByteCount, top.SaveByteCount); }

        internal static string Push(Size top, Size size, BitsConst data)
        {
            const string snippet = "Data.Set(data, {0}, {1});\n";
            return string.Format(snippet, (top - size).SaveByteCount, data.ByteSequence(size));
        }

        internal static string TopRef(Size top, Size size) { return PushVariableReference(top, size, "data", top.SaveByteCount); }

        internal static string LocalVariableReference(Size top, Size size, string holder, Size offset) { return PushVariableReference(top, size, holder, offset.SaveByteCount); }

        private static string PushVariableReference(Size top, Size size, string holder, int offset)
        {
            const string snippet = "Data.SetFromPointer(data, {3}, {0}, {1}, {2});\n";
            return string.Format(snippet, size.SaveByteCount, holder, offset, (top - size).SaveByteCount);
        }

        internal static string Dereference(Size top, Size refSize, Size size)
        {
            const string snippet = "Data.Dereference(data, {2}, {0}, {1})";
            return string.Format(snippet, refSize.SaveByteCount, size.SaveByteCount, top.SaveByteCount);
        }

        internal static string BitCast(Size top, Size size, Size significantSize)
        {
            const string snippet = "Data.BitCast(data, {2}, {0}, {1})";
            return string.Format(snippet, size.SaveByteCount, significantSize, top.SaveByteCount);
        }

        internal static string DumpPrint(Size top, Size size)
        {
            const string snippet = "Data.DumpPrint(data, {1}, {0})";
            return string.Format(snippet, size.SaveByteCount, top.SaveByteCount);
        }

        internal static string CreateBitArray(Size size, BitsConst data)
        {
            var result = "new DataContainer(";
            result += data.ByteSequence(size);
            result += ")";
            return result;
        }

        internal static string DumpPrintText(string text) { return "DataContainer.DumpPrint(" + text.Quote() + ")"; }

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

        internal string CreateLocalBlockEnd(Size size, Size bodySize) { return CreateMoveBytes(size, Start + bodySize, Start); }

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
            {
                return CreateDataRef(destStart, size)
                       + " = "
                       + CreateDataRef(sourceStart, size)
                    ;
            }
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
            {
                return CreateDataRef(destStart, size)
                       + " = "
                       + CreateFrameBackRef(sourceStart, size)
                    ;
            }
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
            {
                return CreateFrameBackRef(destStart, size)
                       + " = "
                       + CreateDataRef(sourceStart, size)
                    ;
            }
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

        private static string Flatten(string holder, CodeBase codeBase)
        {
            var resultHeader = holder + ".Expand({0});\n";
            return codeBase
                .CSharpCodeSnippet()
                .Flatten(resultHeader);
        }

        internal static string TopData(Size offset, Size size) { return "DataContainer.TopData(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")"; }

        internal static string BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize) { return opToken.DataFunctionName + "(" + size.SaveByteCount + ", " + leftSize.SaveByteCount + ")"; }

        internal static string BitArrayPrefix(ISequenceOfBitPrefixOperation opToken, Size size) { return opToken.DataFunctionName + "(" + size.SaveByteCount + ")"; }
        internal static string Drop(Size size, Size outputSize) { return "Drop(" + outputSize.SaveByteCount + ")"; }

        internal static string LocalVariableAccess(string holder, Size offset, Size size) { return holder + ".DataPart(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")"; }

        internal static string Call(int functionIndex) { return "Call(" + Generator.FunctionName(functionIndex) + ")"; }

        internal static string TopFrame(Size offset, Size size) { return Generator.FrameArgName + ".DataPartFromBack(" + offset.SaveByteCount + ", " + size.SaveByteCount + ")"; }
    }
}