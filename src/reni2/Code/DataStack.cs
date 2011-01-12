using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    internal sealed class DataStack: ReniObject, IFormalMaschine, IStackDataAddressBase
    {
        StackData _data = new EmptyStackData();
        [IsDumpEnabled(true)]
        readonly DictionaryEx<string, StackData> _locals = new DictionaryEx<string, StackData>();
        private readonly CodeBase[] _functions;
        private readonly bool _isTraceEnabled;
        public DataStack(CodeBase[] functions, bool isTraceEnabled)
        {
            _functions = functions;
            _isTraceEnabled = isTraceEnabled;
        }

        internal StackData Data { get { return _data; } }

        void IFormalMaschine.BitsArray(Size size, BitsConst data)
        {
            Push(new BitsStackData(data.Resize(size)));
        }

        private void Push(StackData value) { _data = _data.Push(value); }

        void IFormalMaschine.TopRef(RefAlignParam refAlignParam, Size offset)
        {
            _data = _data.Push(new StackDataAddress(this,refAlignParam.RefSize,offset - _data.Size));
        }

        void IFormalMaschine.Call(Size size, int functionIndex, Size argsAndRefsSize)
        {
            NotImplementedMethod(size,functionIndex,argsAndRefsSize);
        }

        void IFormalMaschine.BitCast(Size size, Size targetSize, Size significantSize)
        {
            Tracer.Assert(size == targetSize);
            Push(Pull(targetSize).BitCast(significantSize));
        }

        void IFormalMaschine.DumpPrintOperation(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            Pull(leftSize).DumpPrintOperation();
        }

        void IFormalMaschine.TopFrame(Size offset, Size size, Size dataSize)
        {
            NotImplementedMethod(offset, size, dataSize);
        }

        void IFormalMaschine.TopData(Size offset, Size size, Size dataSize)
        {
            NotImplementedMethod(offset, size, dataSize);
        }

        void IFormalMaschine.LocalBlockEnd(Size size, Size intermediateSize)
        {
            NotImplementedMethod(size, intermediateSize);
        }

        void IFormalMaschine.Drop(Size beforeSize, Size afterSize)
        {
            NotImplementedMethod(beforeSize, afterSize);
        }

        void IFormalMaschine.RefPlus(Size size, Size right)
        {
            Push(Pull(size).RefPlus(right));
        }

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
            NotImplementedMethod(targetSize, refAlignParam);
        }

        void IFormalMaschine.DumpPrintText()
        {
            NotImplementedMethod();
        }

        void IFormalMaschine.List(CodeBase[] data)
        {
            var index = 0;
            foreach (var codeBase in data)
            {
                SubExecute("[" + index + "]", codeBase);
                index++;
            }
        }

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

        internal bool IsTraceEnabled { get { return _isTraceEnabled; } }

        void IFormalMaschine.LocalVariables(string holderNamePattern, CodeBase[] data)
        {
            var index = 0;
            foreach (var codeBase in data)
            {
                var holderName = string.Format(holderNamePattern, index);
                SubExecute(holderName + " =", codeBase);
                var top = Pull(codeBase.Size);
                _locals.Add(holderName, top);
                index++;
            }
        }

        void IFormalMaschine.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            SubExecute("[*]", fiberHead);
            var index = 0;
            foreach (var codeBase in fiberItems)
            {
                SubExecute("[" + index + "]", codeBase);
                index++;
            }
        }

        void IFormalMaschine.LocalVariableReference(Size size, string holder, Size offset)
        {
            Push(new StackDataAddress(new LocalStackReference(_locals,holder),size,offset));
        }

        void IFormalMaschine.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            if(Pull(condSize).GetBitsConst().IsZero)
                SubExecute("else:", elseCode);
            else
                SubExecute("then:", thenCode);
        }

        void IFormalMaschine.LocalVariableAccess(Size size, string holder, Size offset, Size dataSize)
        {
            Push(_locals[holder].DoPull(offset).DoGetTop(size).BitCast(dataSize));
        }

        private StackData Pull(Size size)
        {
            var result = _data.DoGetTop(size);
            _data = _data.DoPull(size);
            return result;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _data.DoPull(_data.Size + offset).DoGetTop(size); }
        string IStackDataAddressBase.Dump() { return "stack"; }
    }
}