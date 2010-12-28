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
        public DataStack(CodeBase[] functions) { _functions = functions; }

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
            NotImplementedMethod(size, targetSize, significantSize);
        }

        void IFormalMaschine.DumpPrintOperation(Size leftSize, Size rightSize)
        {
            NotImplementedMethod(leftSize, rightSize);
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
            NotImplementedMethod(size, right);
        }

        void IFormalMaschine.Dereference(RefAlignParam refAlignParam, Size size, Size dataSize)
        {
            Tracer.Assert(size == dataSize);
            var value = Pull(refAlignParam.RefSize);
            Push(value.Dereference(size));
        }

        void IFormalMaschine.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            NotImplementedMethod(opToken, size, leftSize, rightSize);
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
            Tracer.Line(stars + Dump() + stars);
            Tracer.Line(tag +" " + codeBase.Dump());
            Tracer.IndentStart();
            codeBase.Execute(this);
            Tracer.IndentEnd();
        }

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

        private StackData Pull(Size size)
        {
            var result = _data.GetTop(size);
            _data = _data.Pull(size);
            return result;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _data.Pull(_data.Size + offset).GetTop(size); }
        string IStackDataAddressBase.Dump() { return "stack"; }
    }

    internal sealed class LocalStackReference : ReniObject, IStackDataAddressBase
    {
        private readonly DictionaryEx<string, StackData> _locals;
        private readonly string _holder;

        public LocalStackReference(DictionaryEx<string, StackData> locals, string holder)
        {
            _locals = locals;
            _holder = holder;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size)
        {
            return _locals[_holder].Pull(offset).GetTop(size);
        }

        string IStackDataAddressBase.Dump()
        {
            return _holder;
        }
    }
}