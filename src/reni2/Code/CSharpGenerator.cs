//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Runtime;

namespace Reni.Code
{
    sealed class CSharpGenerator : ReniObject, IVisitor
    {
        readonly int _temporaryByteCount;
        readonly List<string> _data = new List<string>();
        readonly Size _start = Size.Zero;
        int _indent;

        public CSharpGenerator(int temporaryByteCount) { _temporaryByteCount = temporaryByteCount; }

        public string Data
        {
            get
            {
                var start = string.Format("\nvar data = Data.Create({0})", _temporaryByteCount);
                return _data
                           .Aggregate(start, (x, y) => x + ";\n" + y)
                       + ";\n";
            }
        }

        [StringFormatMethod("pattern")]
        void AddCode(string pattern, params object[] data)
        {
            var c = string.Format(pattern, data);
            Tracer.ConditionalBreak(c == "data.==(1, 1);", () => "");
            _data.Add("    ".Repeat(_indent) + c);
        }

        string BitCast(Size size, Size dataSize)
        {
            if(size == dataSize)
                return "";
            return string.Format(".BitCast({0}).BitCast({1})", dataSize.ToInt(), size.ToInt());
        }

        static int RefBytes { get { return DataHandler.RefBytes; } }

        void IVisitor.Call(Size size, int functionIndex, Size argsAndRefsSize) { throw new NotImplementedException(); }
        void IVisitor.Drop(Size beforeSize, Size afterSize) { throw new NotImplementedException(); }
        void IVisitor.LocalBlockEnd(Size size, Size intermediateSize) { throw new NotImplementedException(); }
        void IVisitor.RecursiveCall() { throw new NotImplementedException(); }
        void IVisitor.ReferenceCode(IReferenceInCode context) { throw new NotImplementedException(); }
        void IVisitor.TopData(Size offset, Size size, Size dataSize) { throw new NotImplementedException(); }
        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize) { throw new NotImplementedException(); }
        void IVisitor.TopFrameRef(Size offset) { throw new NotImplementedException(); }

        void IVisitor.BitsArray(Size size, BitsConst data) { AddCode("data.Push({0})", data.ByteSequence(size)); }
        void IVisitor.LocalVariableDefinition(string holderName, Size valueSize) { AddCode("var {0} = data.Pull({1})", holderName, valueSize.SaveByteCount); }
        void IVisitor.LocalVariableReference(string holder, Size offset) { AddCode("data.Push({0}.Address({1}))", holder, offset.SaveByteCount); }
        void IVisitor.RefPlus(Size size) { AddCode("data.RefPlus({0})", size.SaveByteCount); }
        void IVisitor.PrintNumber(Size leftSize, Size rightSize) { AddCode("data.Pull({0}).PrintNumber()", leftSize.SaveByteCount); }
        void IVisitor.PrintText(string dumpPrintText) { AddCode("Data.PrintText({0})", dumpPrintText.Quote()); }
        void IVisitor.TopRef(Size offset) { AddCode("data.Push(data.Address({0}))", offset.SaveByteCount); }
        void IVisitor.Assign(Size targetSize) { AddCode("data.Assign({0})", targetSize.SaveByteCount); }
        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize) { AddCode("data.Push(data.Pull({0}).BitCast({1}).BitCast({2}))", targetSize.SaveByteCount, significantSize.ToInt(), size.ToInt()); }
        void IVisitor.PrintText(Size leftSize, Size itemSize) { AddCode("data.Pull({0}).PrintText({1})", leftSize.SaveByteCount, itemSize.SaveByteCount); }

        void IVisitor.LocalVariableAccess(string holder, Size offset, Size size, Size dataSize)
        {
            AddCode
                ("data.Push({0}.Get({1}, {2}){3})"
                 , holder
                 , dataSize.ByteCount
                 , offset.SaveByteCount
                 , BitCast(size, dataSize)
                );
        }

        void IVisitor.Dereference(Size size, Size dataSize)
        {
            AddCode
                ("data.Push(data.Pull({0}).Dereference({1}){2})"
                 , RefBytes
                 , dataSize.ByteCount
                 , BitCast(size, dataSize)
                );
        }

        void IVisitor.BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize)
        {
            var sizeBytes = size.SaveByteCount;
            var argBytes = argSize.SaveByteCount;
            if(sizeBytes == argBytes)
                AddCode("data.{0}Prefix(bytes:{1})", opToken.DataFunctionName, sizeBytes);
            else
                AddCode
                    ("data.{0}Prefix(sizeBytes:{1}, argBytes:{2})"
                     , opToken.DataFunctionName
                     , sizeBytes
                     , argBytes
                    );
        }

        void IVisitor.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var sizeBytes = size.SaveByteCount;
            var leftBytes = leftSize.SaveByteCount;
            var rightBytes = rightSize.SaveByteCount;
            if(sizeBytes == 1)
                AddCode
                    ("data.{0}(leftBytes:{1}, rightBytes:{2})"
                     , opToken.DataFunctionName
                     , leftBytes
                     , rightBytes
                    );
            else
                AddCode
                    ("data.{0}(sizeBytes:{1}, leftBytes:{2}, rightBytes:{3})"
                     , opToken.DataFunctionName
                     , sizeBytes
                     , leftBytes
                     , rightBytes
                    );
        }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            AddCode("if({0})\n{{", PullBool(condSize.SaveByteCount));
            Indent();
            thenCode.Visit(this);
            Unindent();
            AddCode("}}\nelse\n{{");
            Indent();
            elseCode.Visit(this);
            Unindent();
            AddCode("}}");
        }
        
        string PullBool(int saveByteCount) {
            if (saveByteCount == 1)
                return "data.Pull(1).GetBytes()[0] != 0";
            return "data.Pull(" + saveByteCount + ").IsNotNull()";
        }

        void Unindent() { _indent--; }
        void Indent() { _indent++; }

        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            fiberHead.Visit(this);
            foreach(var fiberItem in fiberItems)
                fiberItem.Visit(this);
        }

        void IVisitor.List(CodeBase[] data)
        {
            foreach(var codeBase in data)
                codeBase.Visit(this);
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

        static string Indented(string tag, int objectId, string result) { return "//" + tag + " " + objectId + ("\n" + result).Indent() + "\n"; }

        internal static string Fiber(Size top, int objectId, FiberItem[] fiberItems, FiberHead fiberHead)
        {
            var result = "";
            result += "/* " + fiberHead.NodeDump + " */\n";
            result += fiberHead.ReversePolish(top);
            top -= fiberHead.Size;
            foreach(var fiberItem in fiberItems)
            {
                result += "/* " + fiberItem.NodeDump + " */\n";
                top -= fiberItem.DeltaSize;
            }
            return Indented("Fiber", objectId, result);
        }


        internal static string Push(Size top, Size size, BitsConst data)
        {
            const string snippet = "Data.Set(data, {0}, {1});\n";
            return string.Format(snippet, (top - size).SaveByteCount, data.ByteSequence(size));
        }

        internal static string TopRef(Size top, Size size) { return PushVariableReference(top, size, "data", top.SaveByteCount); }

        internal static string LocalVariableReference(Size top, Size size, string holder, Size offset) { return PushVariableReference(top, size, holder, offset.SaveByteCount); }

        static string PushVariableReference(Size top, Size size, string holder, int offset)
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

        internal static string DumpPrintText(string text) { return "DataContainer.DumpPrint(" + text.Quote() + ")"; }

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

        Size Start { get { return _start; } }

        internal string CreateLocalBlockEnd(Size size, Size bodySize) { return CreateMoveBytes(size, Start + bodySize, Start); }

        internal string CreateThen(Size condSize) { return "if(" + CreateDataRef(Start, condSize) + "!=0) {"; }

        internal static string CreateFrameRef(RefAlignParam refAlignParam, Size offset)
        {
            NotImplementedFunction(null, refAlignParam, offset);
            return null;
        }

        static string CreateCastToIntRef(Size size, string result) { return "(*" + CreateIntPtrCast(size) + " " + result + ")"; }

        static string CreateDataPtr(Size start) { return "(data+" + start.SaveByteCount + ")"; }

        static string CreateDataRef(Size start, Size size) { return CreateCastToIntRef(size, CreateDataPtr(start)); }

        static string CreateFrameBackPtr(Size start) { return "(frame-" + start.SaveByteCount + ")"; }

        static string CreateFrameBackRef(Size start, Size size) { return CreateCastToIntRef(size, CreateFrameBackPtr(start)); }

        static string CreateIntPtrCast(Size size) { return "(" + CreateIntType(size) + "*)"; }

        static string CreateIntType(Size size)
        {
            var bits = size.ByteCount * 8;
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

        static string CreateMoveBytes(Size size, Size destStart, Size sourceStart)
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

        static string CreateMoveBytesFromFrame(Size size, Size destStart, Size sourceStart)
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

        static string CreateMoveBytesToFrame(Size size, Size destStart, Size sourceStart)
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

        static bool IsBuildInIntType(Size size)
        {
            var bits = size.ByteCount * 8;
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

        const string DataName = "data";
        internal const string VarDataNewDatacontainer = "var " + DataName + " = new DataContainer();\n";

        static string Flatten(string holder, CodeBase codeBase)
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