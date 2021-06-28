﻿using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;

namespace Reni.Code
{
    sealed class DataStack : DumpableObject, IVisitor
    {
        sealed class LocalData : DumpableObject, IStackDataAddressBase
        {
            public StackData Data;
            public FrameData Frame = new FrameData(null);
            public LocalData(IOutStream outStream) { Data = new EmptyStackData(outStream); }

            string IStackDataAddressBase.Dump() => "stack";

            StackData IStackDataAddressBase.GetTop(Size offset, Size size)
                => Data.DoPull(Data.Size + offset).DoGetTop(size);

            void IStackDataAddressBase.SetTop(Size offset, StackData right)
            {
                var oldTop = Data.DoGetTop(Data.Size + offset);
                var tail = Data.DoPull(Data.Size + offset + right.Size);
                var newDataTail = tail.Push(right);
                var newData = newDataTail.Push(oldTop);
                Data = newData;
            }

            internal StackDataAddress Address(Size offset)
                => new StackDataAddress(this, offset - Data.Size, Data.OutStream);

            internal StackData FrameAddress(Size offset)
                => new StackDataAddress(Frame, offset, Data.OutStream);
        }

        internal static Size RefSize => Root.DefaultRefAlignParam.RefSize;

        readonly IExecutionContext _context;
        [DisableDump]
        internal ITraceCollector TraceCollector;
        [EnableDump]
        LocalData _localData;

        public DataStack(IExecutionContext context)
        {
            _context = context;
            _localData = new LocalData(_context.OutStream);
        }

        internal IEnumerable<DataMemento> GetLocalItemMementos() => Data.GetItemMementos();

        [DisableDump]
        internal BitsConst Value => Data.GetBitsConst();

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        {
            var oldFrame = _localData.Frame;
            var argsAndRefs = Pull(argsAndRefsSize);
            TraceCollector.Call(argsAndRefs, functionId);
            do
            {
                _localData.Frame = new FrameData(argsAndRefs);
                SubVisit(_context.Function(functionId));
            } while(_localData.Frame.IsRepeatRequired);
            TraceCollector.Return();
            _localData.Frame = oldFrame;
        }

        void IVisitor.RecursiveCall() { _localData.Frame.IsRepeatRequired = true; }

        StackData Data { get { return _localData.Data; } set { _localData.Data = value; } }

        internal Size Size
        {
            get { return Data.Size; }
            set
            {
                if(Size == value)
                    return;
                if (Size < value)
                    Push(new UnknownStackData(value - Size, Data.OutStream));
                else
                    Data = Data.ForcedPull(Size-value);
            }
        }

        void IVisitor.BitsArray(Size size, BitsConst data)
        {
            if(size.IsZero)
                return;
            Push(new BitsStackData(data.Resize(size), _context.OutStream));
        }

        void Push(StackData value) { Data = Data.Push(value); }

        void IVisitor.TopRef(Size offset) => Push(_localData.Address(offset));
        void IVisitor.TopFrameRef(Size offset) => Push(_localData.FrameAddress(offset));

        void IVisitor.ArrayGetter(Size elementSize, Size indexSize)
        {
            var offset = elementSize * Pull(indexSize).GetBitsConst().ToInt32();
            var baseAddress = Pull(RefSize);
            Push(baseAddress.RefPlus(offset));
        }

        void IVisitor.ArraySetter(Size elementSize, Size indexSize)
        {
            var right = Pull(RefSize);
            var offset = elementSize * Pull(indexSize).GetBitsConst().ToInt32();
            var baseAddress = Pull(RefSize);
            var left = baseAddress.RefPlus(offset);
            left.Assign(elementSize, right);
        }

        void IVisitor.RecursiveCallCandidate() { throw new NotImplementedException(); }

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        {
            var frame = _localData.Frame.Data;
            var value = frame
                .DoPull(offset)
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
            TracerAssert
                (
                    size == targetSize,
                    () =>
                        nameof(size) + " == " + nameof(targetSize) +
                            " " + nameof(size) + "=" + size +
                            " " + nameof(targetSize) + "=" + targetSize);
            Push(Pull(targetSize).BitCast(significantSize).BitCast(size));
        }

        void TracerAssert(bool condition, Func<string> dumper)
        {
            if(TraceCollector == null)
            {
                Tracer.Assert(condition, dumper, 1);
                return;
            }

            if(condition)
                return;

            TraceCollector.AssertionFailed(dumper, 1);
        }

        void IVisitor.PrintNumber(Size leftSize, Size rightSize)
        {
            TracerAssert(rightSize.IsZero, () => "rightSize.IsZero");
            Pull(leftSize).PrintNumber();
        }

        void IVisitor.PrintText(Size size, Size itemSize) => Pull(size).PrintText(itemSize);

        void IVisitor.Drop(Size beforeSize, Size afterSize)
        {
            var top = Data.DoGetTop(afterSize);
            Pull(beforeSize);
            Push(top);
        }

        void IVisitor.ReferencePlus(Size right) => Push(Pull(RefSize).RefPlus(right));

        void IVisitor.DePointer(Size size, Size dataSize)
        {
            var value = Pull(RefSize);
            Push(value.Dereference(dataSize, dataSize).BitCast(size));
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
            foreach(var codeBase in data)
                SubVisit(codeBase);
        }

        void SubVisit(IFormalCodeItem codeBase)
        {
            if(TraceCollector == null)
                codeBase.Visit(this);
            else
                TraceCollector.Run(this, codeBase);
        }

        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            SubVisit(fiberHead);
            foreach(var codeBase in fiberItems)
                SubVisit(codeBase);
        }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            var bitsConst = Pull(condSize).GetBitsConst();
            SubVisit(bitsConst.IsZero ? elseCode : thenCode);
        }

        StackData Pull(Size size)
        {
            var result = Data.DoGetTop(size);
            Data = Data.DoPull(size);
            return result;
        }

        internal sealed class DataMemento
        {
            internal DataMemento(string valueDump) { ValueDump = valueDump; }
            internal int Offset;
            internal int Size;
            readonly internal string ValueDump;
        }
    }

    interface IExecutionContext
    {
        IOutStream OutStream { get; }
        CodeBase Function(FunctionId functionId);
    }

    interface ITraceCollector
    {
        void AssertionFailed(Func<string> dumper, int depth);
        void Run(DataStack dataStack, IFormalCodeItem codeBase);
        void Return();
        void Call(StackData argsAndRefs, FunctionId functionId);
    }
}