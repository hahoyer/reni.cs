// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
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
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class DataStack : ReniObject, IVisitor
    {
        sealed class LocalData : ReniObject, IStackDataAddressBase
        {
            public StackData Data = new EmptyStackData();
            public FrameData Frame = new FrameData(null);

            string IStackDataAddressBase.Dump() { return "stack"; }

            StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return Data.DoPull(Data.Size + offset).DoGetTop(size); }

            void IStackDataAddressBase.SetTop(Size offset, StackData right)
            {
                var oldTop = Data.DoGetTop(Data.Size + offset);
                Data = Data
                    .DoPull(Data.Size + offset + right.Size)
                    .Push(right)
                    .Push(oldTop);
            }

            internal StackDataAddress Address(Size offset) { return new StackDataAddress(this, offset - Data.Size); }

            internal StackData FrameAddress(Size offset) { return new StackDataAddress(Frame, offset); }
        }

        internal static Size RefSize { get { return Root.DefaultRefAlignParam.RefSize; } }

        [EnableDump]
        LocalData _localData = new LocalData();

        readonly CodeBase[] _functions;
        readonly bool _isTraceEnabled;

        public DataStack(CodeBase[] functions, bool isTraceEnabled)
        {
            _functions = functions;
            _isTraceEnabled = isTraceEnabled;
        }

        [DisableDump]
        internal BitsConst Value { get { return Data.GetBitsConst(); } }

        void IVisitor.Call(Size size, int functionIndex, Size argsAndRefsSize)
        {
            var oldFrame = _localData.Frame;
            var argsAndRefs = Pull(argsAndRefsSize);
            do
            {
                _localData.Frame = new FrameData(argsAndRefs);
                SubVisit("call " + functionIndex, _functions[functionIndex]);
            } while(_localData.Frame.IsRepeatRequired);
            _localData.Frame = oldFrame;
        }

        void IVisitor.RecursiveCall() { _localData.Frame.IsRepeatRequired = true; }

        StackData Data { get { return _localData.Data; } set { _localData.Data = value; } }

        void IVisitor.BitsArray(Size size, BitsConst data)
        {
            if(size.IsZero)
                return;
            Push(new BitsStackData(data.Resize(size)));
        }

        void Push(StackData value) { Data = Data.Push(value); }

        void IVisitor.TopRef(Size offset) { Push(_localData.Address(offset)); }
        void IVisitor.TopFrameRef(Size offset) { Push(_localData.FrameAddress(offset)); }

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

        void IVisitor.PrintText(Size size, Size itemSize) { Pull(size).PrintText(itemSize); }

        void IVisitor.LocalBlockEnd(Size size, Size intermediateSize) { NotImplementedMethod(size, intermediateSize); }

        void IVisitor.Drop(Size beforeSize, Size afterSize) { NotImplementedMethod(beforeSize, afterSize); }

        void IVisitor.RefPlus(Size right) { Push(Pull(RefSize).RefPlus(right)); }

        void IVisitor.Dereference(Size size, Size dataSize)
        {
            var value = Pull(RefSize);
            Push(value.Dereference(size, dataSize));
        }

        void IVisitor.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var right = Pull(rightSize);
            var left = Pull(leftSize);
            Push(left.BitArrayBinaryOp(opToken, size, right));
        }

        public void BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize)
        {
            var arg = Pull(argSize);
            Push(arg.BitArrayPrefixOp(opToken, size));
        }

        void IVisitor.Assign(Size targetSize)
        {
            var right = Pull(RefSize);
            var left = Pull(RefSize);
            left.Assign(targetSize, right);
        }

        void IVisitor.PrintText(string dumpPrintText) { BitsConst.OutStream.Add(dumpPrintText); }

        void IVisitor.List(CodeBase[] data)
        {
            var index = 0;
            foreach(var codeBase in data)
            {
                SubVisit("[" + index + "]", codeBase);
                index++;
            }
        }

        void IVisitor.ReferenceCode(IReferenceInCode context) { throw new UnexpectedContextReference(context); }

        void IVisitor.LocalVariableDefinition(string holderName, Size valueSize)
        {
            Locals
                .Add(holderName, Pull(valueSize));
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

        bool IsTraceEnabled { get { return _isTraceEnabled; } }

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

        void IVisitor.LocalVariableReference(string holder, Size offset) { Push(new StackDataAddress(new LocalStackReference(Locals, holder), offset)); }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            var bitsConst = Pull(condSize).GetBitsConst();
            if(bitsConst.IsZero)
                SubVisit("else:", elseCode);
            else
                SubVisit("then:", thenCode);
        }

        DictionaryEx<string, StackData> Locals { get { return _localData.Frame.Locals; } }

        void IVisitor.LocalVariableAccess(string holder, Size offset, Size size, Size dataSize) { Push(Locals[holder].DoPull(offset).DoGetTop(dataSize).BitCast(size)); }

        StackData Pull(Size size)
        {
            var result = Data.DoGetTop(size);
            Data = Data.DoPull(size);
            return result;
        }
    }
}