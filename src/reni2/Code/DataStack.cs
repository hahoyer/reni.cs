using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    internal sealed class DataStack : ReniObject, IFormalMaschine
    {
        private sealed class LocalData : ReniObject, IStackDataAddressBase
        {
            public StackData Data = new EmptyStackData();
            public readonly DictionaryEx<string, StackData> Locals = new DictionaryEx<string, StackData>();
            public FrameData Frame;

            string IStackDataAddressBase.Dump() { return "stack"; }

            StackData IStackDataAddressBase.GetTop(Size offset, Size size)
            {
                return Data.DoPull(Data.Size + offset).DoGetTop(size);
            }

            void IStackDataAddressBase.SetTop(Size offset, StackData right)
            {
                var oldTop = Data.DoGetTop(Data.Size + offset);
                Data = Data
                    .DoPull(Data.Size + offset + right.Size)
                    .Push(right)
                    .Push(oldTop);
            }
            internal StackDataAddress Address(Size offset, Size size) { return new StackDataAddress(this, size, offset - Data.Size); }

            internal StackData FrameAddress(Size offset, Size size, Size dataSize) { return new StackDataAddress(Frame, size, offset); }
        }

        [IsDumpEnabled(true)]
        private LocalData _localData = new LocalData();

        private readonly CodeBase[] _functions;
        private readonly bool _isTraceEnabled;

        public DataStack(CodeBase[] functions, bool isTraceEnabled)
        {
            _functions = functions;
            _isTraceEnabled = isTraceEnabled;
        }

        private StackData Data { get { return _localData.Data; } set { _localData.Data = value; } }

        void IFormalMaschine.BitsArray(Size size, BitsConst data)
        {
            if(size.IsZero)
                return;
            Push(new BitsStackData(data.Resize(size)));
        }

        private void Push(StackData value) { Data = Data.Push(value); }

        void IFormalMaschine.TopRef(Size offset, Size size)
        {
            var address = _localData.Address(offset, size);
            Data = Data.Push(address);
        }

        void IFormalMaschine.TopFrameData(Size offset, Size size, Size dataSize) { Push(_localData.Frame.Data.DoPull(_localData.Frame.Data.Size + offset).DoGetTop(size).BitCast(dataSize)); }

        void IFormalMaschine.TopData(Size offset, Size size, Size dataSize) { NotImplementedMethod(offset, size, dataSize); }

        void IFormalMaschine.Call(Size size, int functionIndex, Size argsAndRefsSize)
        {
            var oldFrame = _localData.Frame;
            _localData.Frame = new FrameData(Pull(argsAndRefsSize));
            SubExecute("call " + functionIndex, _functions[functionIndex]);
            _localData.Frame = oldFrame;
        }

        void IFormalMaschine.BitCast(Size size, Size targetSize, Size significantSize)
        {
            Tracer.Assert(size == targetSize);
            Push(Pull(targetSize).BitCast(significantSize).BitCast(size));
        }

        void IFormalMaschine.DumpPrintOperation(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            Pull(leftSize).DumpPrintOperation();
        }

        void IFormalMaschine.LocalBlockEnd(Size size, Size intermediateSize) { NotImplementedMethod(size, intermediateSize); }

        void IFormalMaschine.Drop(Size beforeSize, Size afterSize) { NotImplementedMethod(beforeSize, afterSize); }

        void IFormalMaschine.RefPlus(Size size, Size right) { Push(Pull(size).RefPlus(right)); }

        void IFormalMaschine.Dereference(RefAlignParam refAlignParam, Size size, Size dataSize)
        {
            var value = Pull(refAlignParam.RefSize);
            Push(value.Dereference(size, dataSize));
        }

        void IFormalMaschine.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var right = Pull(rightSize);
            var left = Pull(leftSize);
            Push(left.BitArrayBinaryOp(opToken, size, right));
        }

        void IFormalMaschine.Assign(Size targetSize, RefAlignParam refAlignParam)
        {
            var right = Pull(refAlignParam.RefSize);
            var left = Pull(refAlignParam.RefSize);
            left.Assign(targetSize, right);
        }

        void IFormalMaschine.DumpPrintText(string dumpPrintText) { BitsConst.OutStream.Add(dumpPrintText); }

        void IFormalMaschine.List(CodeBase[] data)
        {
            var index = 0;
            foreach(var codeBase in data)
            {
                SubExecute("[" + index + "]", codeBase);
                index++;
            }
        }

        void IFormalMaschine.ReferenceCode(IReferenceInCode context) { throw new UnexpectedContextReference(context); }

        void IFormalMaschine.LocalVariableDefinition(string holderName, Size valueSize) { Locals.Add(holderName, Pull(valueSize)); }

        private void SubExecute(string tag, IFormalCodeItem codeBase)
        {
            const string stars = "\n******************************\n";
            if(IsTraceEnabled)
            {
                Tracer.Line(stars + Dump() + stars);
                Tracer.Line(tag + " " + codeBase.Dump());
                Tracer.IndentStart();
            }
            codeBase.Execute(this);
            if(IsTraceEnabled)
                Tracer.IndentEnd();
        }

        private bool IsTraceEnabled { get { return _isTraceEnabled; } }

        void IFormalMaschine.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            SubExecute("[*]", fiberHead);
            var index = 0;
            foreach(var codeBase in fiberItems)
            {
                SubExecute("[" + index + "]", codeBase);
                index++;
            }
        }

        void IFormalMaschine.LocalVariableReference(Size size, string holder, Size offset) { Push(new StackDataAddress(new LocalStackReference(Locals, holder), size, offset)); }

        void IFormalMaschine.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            var bitsConst = Pull(condSize).GetBitsConst();
            if(bitsConst.IsZero)
                SubExecute("else:", elseCode);
            else
                SubExecute("then:", thenCode);
        }

        private DictionaryEx<string, StackData> Locals { get { return _localData.Locals; } }

        void IFormalMaschine.LocalVariableData(Size size, string holder, Size offset, Size dataSize) { Push(Locals[holder].DoPull(offset).DoGetTop(dataSize).BitCast(size)); }

        private StackData Pull(Size size)
        {
            var result = Data.DoGetTop(size);
            Data = Data.DoPull(size);
            return result;
        }
    }
}