using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Runtime;
using Reni.Struct;

namespace Reni.Code
{
    sealed class CSharpGenerator : DumpableObject, IVisitor
    {
        readonly int _temporaryByteCount;
        readonly List<string> _data = new List<string>();
        int _indent;

        public CSharpGenerator(int temporaryByteCount) { _temporaryByteCount = temporaryByteCount; }

        public string Data
        {
            get
            {
                var start = $"\nvar data = Data.Create({_temporaryByteCount})";
                return _data
                    .Aggregate(start, (x, y) => x + ";\n" + y)
                    + ";\n";
            }
        }

        [StringFormatMethod("pattern")]
        void AddCode(string pattern, params object[] data)
        {
            var c = string.Format(pattern, data);
            _data.Add("    ".Repeat(_indent) + c);
        }

        static string BitCast(Size size, Size dataSize)
        {
            if(size == dataSize)
                return "";
            return $".BitCast({dataSize.ToInt()}).BitCast({size.ToInt()})";
        }

        static int RefBytes => DataHandler.RefBytes;


        void IVisitor.Drop(Size beforeSize, Size afterSize)
        {
            if(afterSize.IsZero)
                AddCode("data.Drop({0})", beforeSize.ByteCount);
            else
                AddCode("data.Drop({0}, {1})", beforeSize.ByteCount, afterSize.ByteCount);
        }

        void IVisitor.BitsArray(Size size, BitsConst data)
            => AddCode("data.SizedPush({0}{1})", size.ByteCount, data.ByteSequence());

        void IVisitor.ReferencePlus(Size size)
            => AddCode("data.PointerPlus({0})", size.SaveByteCount);

        void IVisitor.PrintNumber(Size leftSize, Size rightSize)
            => AddCode("data.Pull({0}).PrintNumber()", leftSize.SaveByteCount);

        void IVisitor.PrintText(string dumpPrintText)
            => AddCode("Data.PrintText({0})", dumpPrintText.Quote());

        void IVisitor.TopRef(Size offset)
            => AddCode("data.Push(data.Pointer({0}))", offset.SaveByteCount);

        void IVisitor.TopFrameRef(Size offset)
            => AddCode("data.Push(frame.Pointer({0}))", offset.SaveByteCount);

        void IVisitor.Assign(Size targetSize)
            => AddCode("data.Assign({0})", targetSize.SaveByteCount);

        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
            => AddCode
                (
                    "data.Push(data.Pull({0}).BitCast({1}).BitCast({2}))",
                    targetSize.SaveByteCount,
                    significantSize.ToInt(),
                    size.ToInt()
                );

        void IVisitor.PrintText(Size leftSize, Size itemSize)
            => AddCode
                (
                    "data.Pull({0}).PrintText({1})",
                    leftSize.SaveByteCount,
                    itemSize.SaveByteCount
                );

        void IVisitor.RecursiveCall() => AddCode("goto Start");
        void IVisitor.RecursiveCallCandidate() { throw new UnexpectedRecursiveCallCandidate(); }

        void IVisitor.ArrayGetter(Size elementSize, Size indexSize)
            =>
                AddCode
                    (
                        "data.ArrayGetter({0},{1})",
                        elementSize.SaveByteCount,
                        indexSize.SaveByteCount
                    );

        void IVisitor.ArraySetter(Size elementSize, Size indexSize)
            =>
                AddCode
                    (
                        "data.ArraySetter({0},{1})",
                        elementSize.SaveByteCount,
                        indexSize.SaveByteCount
                    );

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
            => AddCode
                (
                    "data.Push({0}(data.Pull({1})))",
                    Generator.FunctionName(functionId),
                    argsAndRefsSize.SaveByteCount
                );

        void IVisitor.TopData(Size offset, Size size, Size dataSize)
            => AddCode
                (
                    "data.Push(data.Get({0}, {1}){2})",
                    dataSize.ByteCount,
                    offset.SaveByteCount,
                    BitCast(size, dataSize)
                );

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
            => AddCode
                (
                    "data.Push(frame.Get({0}, {1}){2})",
                    dataSize.ByteCount,
                    offset.SaveByteCount,
                    BitCast(size, dataSize)
                );

        void IVisitor.DePointer(Size size, Size dataSize)
            => AddCode
                (
                    "data.Push(data.Pull({0}).DePointer({1}){2})",
                    RefBytes,
                    dataSize.ByteCount,
                    BitCast(size, dataSize)
                );

        void IVisitor.BitArrayPrefixOp(string operation, Size size, Size argSize)
        {
            var sizeBytes = size.SaveByteCount;
            var argBytes = argSize.SaveByteCount;
            if(sizeBytes == argBytes)
                AddCode("data.{0}Prefix(bytes:{1})", operation, sizeBytes);
            else
                AddCode
                    (
                        "data.{0}Prefix(sizeBytes:{1}, argBytes:{2})",
                        operation,
                        sizeBytes,
                        argBytes
                    );
        }

        void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
        {
            var sizeBytes = size.SaveByteCount;
            var leftBytes = leftSize.SaveByteCount;
            var rightBytes = rightSize.SaveByteCount;
            AddCode
                (
                    "data.{0}(sizeBytes:{1}, leftBytes:{2}, rightBytes:{3})",
                    opToken,
                    sizeBytes,
                    leftBytes,
                    rightBytes
                );
        }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            AddCode("if({0})\n{{", PullBool(condSize.ByteCount));
            Indent();
            thenCode.Visit(this);
            Unindent();
            AddCode("}}\nelse\n{{");
            Indent();
            elseCode.Visit(this);
            Unindent();
            AddCode("}}");
        }

        static string PullBool(int byteCount)
        {
            if(byteCount == 1)
                return "data.Pull(1).GetBytes()[0] != 0";
            return "data.Pull(" + byteCount + ").IsNotNull()";
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

        internal static string GenerateCSharpStatements(CodeBase codeBase)
        {
            var generator = new CSharpGenerator(codeBase.TemporarySize.SaveByteCount);
            try
            {
                codeBase.Visit(generator);
            }
            catch(UnexpectedContextReference e)
            {
                Tracer.AssertionFailed("", () => e.Message);
            }
            return generator.Data;
        }
    }
}