using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Code
{
    sealed class DataStack : DumpableObject, IVisitor
    {
        private readonly IExecutionContext _context;

        sealed class LocalData : DumpableObject, IStackDataAddressBase
        {
            public StackData Data;
            public FrameData Frame = new FrameData(null);
            public LocalData(IOutStream outStream) { Data = new EmptyStackData(outStream); }

            string IStackDataAddressBase.Dump() => "stack";

            StackData IStackDataAddressBase.GetTop(Size offset, Size size) => Data.DoPull(Data.Size + offset).DoGetTop(size);

            void IStackDataAddressBase.SetTop(Size offset, StackData right)
            {
                var oldTop = Data.DoGetTop(Data.Size + offset);
                Data = Data
                    .DoPull(Data.Size + offset + right.Size)
                    .Push(right)
                    .Push(oldTop);
            }

            internal StackDataAddress Address(Size offset) => new StackDataAddress(this, offset - Data.Size, Data.OutStream);

            internal StackData FrameAddress(Size offset) => new StackDataAddress(Frame, offset, Data.OutStream);
        }

        internal static Size RefSize => Root.DefaultRefAlignParam.RefSize;

        [EnableDump]
        LocalData _localData;

        public DataStack(IExecutionContext context)
        {
            _context = context;
            _localData = new LocalData(_context.OutStream);
        }

        [DisableDump]
        internal BitsConst Value => Data.GetBitsConst();

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        {
            var oldFrame = _localData.Frame;
            var argsAndRefs = Pull(argsAndRefsSize);
            do
            {
                _localData.Frame = new FrameData(argsAndRefs);
                SubVisit("call " + functionId, _context.Function(functionId));
            } while(_localData.Frame.IsRepeatRequired);
            _localData.Frame = oldFrame;
        }

        void IVisitor.RecursiveCall() { _localData.Frame.IsRepeatRequired = true; }

        StackData Data { get { return _localData.Data; } set { _localData.Data = value; } }

        void IVisitor.BitsArray(Size size, BitsConst data)
        {
            if(size.IsZero)
                return;
            Push(new BitsStackData(data.Resize(size), _context.OutStream));
        }

        void Push(StackData value) { Data = Data.Push(value); }

        void IVisitor.TopRef(Size offset) => Push(_localData.Address(offset));
        void IVisitor.TopFrameRef(Size offset) => Push(_localData.FrameAddress(offset));
        void IVisitor.RecursiveCallCandidate() { throw new NotImplementedException(); }

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        {
            var frame = _localData.Frame.Data;
            var value = frame
                .DoPull(frame.Size + offset)
                .DoGetTop(size)
                .BitCast(dataSize)
                .BitCast(size);
            Push(value);
        }

        void IVisitor.TopData(Size offset, Size size, Size dataSize)
        {
            var value = Data
                .DoPull(offset)
                .DoGetTop(dataSize)
                .BitCast(size);
            Push(value);
        }

        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
        {
            Tracer.Assert(size == targetSize);
            Push(Pull(targetSize).BitCast(significantSize).BitCast(size));
        }

        void IVisitor.PrintNumber(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            Pull(leftSize).PrintNumber();
        }

        void IVisitor.PrintText(Size size, Size itemSize) => Pull(size).PrintText(itemSize);

        void IVisitor.LocalBlockEnd(Size size, Size intermediateSize) => NotImplementedMethod(size, intermediateSize);

        void IVisitor.Drop(Size beforeSize, Size afterSize) => NotImplementedMethod(beforeSize, afterSize);

        void IVisitor.ReferencePlus(Size right) => Push(Pull(RefSize).RefPlus(right));

        void IVisitor.DePointer(Size size, Size dataSize)
        {
            var value = Pull(RefSize);
            Push(value.Dereference(size, dataSize));
        }

        void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
        {
            var right = Pull(rightSize);
            var left = Pull(leftSize);
            Push(left.BitArrayBinaryOp(opToken, size, right));
        }

        public void BitArrayPrefixOp(string operation, Size size, Size argSize)
        {
            var arg = Pull(argSize);
            Push(arg.BitArrayPrefixOp(operation, size));
        }

        void IVisitor.Assign(Size targetSize)
        {
            var right = Pull(RefSize);
            var left = Pull(RefSize);
            left.Assign(targetSize, right);
        }

        void IVisitor.PrintText(string dumpPrintText) => _context.OutStream.AddData(dumpPrintText);

        void IVisitor.List(CodeBase[] data)
        {
            var index = 0;
            foreach(var codeBase in data)
            {
                SubVisit("[" + index + "]", codeBase);
                index++;
            }
        }

        void SubVisit(string tag, IFormalCodeItem codeBase)
        {
            const string stars = "\n******************************\n";
            if(IsTraceEnabled)
            {
                Tracer.Line(stars + Dump() + stars);
                Tracer.Line(tag + " " + codeBase.Dump());
                Tracer.IndentStart();
            }
            codeBase.Visit(this);
            if(IsTraceEnabled)
                Tracer.IndentEnd();
        }

        bool IsTraceEnabled => _context.IsTraceEnabled;

        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            SubVisit("[*]", fiberHead);
            var index = 0;
            foreach(var codeBase in fiberItems)
            {
                SubVisit("[" + index + "]", codeBase);
                index++;
            }
        }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            var bitsConst = Pull(condSize).GetBitsConst();
            if(bitsConst.IsZero)
                SubVisit("else:", elseCode);
            else
                SubVisit("then:", thenCode);
        }

        FunctionCache<string, StackData> Locals => _localData.Frame.Locals;

        StackData Pull(Size size)
        {
            var result = Data.DoGetTop(size);
            Data = Data.DoPull(size);
            return result;
        }
    }

    internal interface IExecutionContext
    {
        IOutStream OutStream { get; }
        bool IsTraceEnabled { get; }
        bool ProcessErrors { get; }
        CodeBase Function(FunctionId functionId);
        Checked<CompileSyntax> Parse(string source);
    }
}