﻿//     Compiler for programming language "Reni"
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
using Reni.Basics;

namespace Reni.Code
{
    internal interface IFormalMaschine
    {
        void Assign(Size targetSize, RefAlignParam refAlignParam);
        void BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize);
        void BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize);
        void BitCast(Size size, Size targetSize, Size significantSize);
        void BitsArray(Size size, BitsConst data);
        void Call(Size size, int functionIndex, Size argsAndRefsSize);
        void Dereference(RefAlignParam refAlignParam, Size size, Size dataSize);
        void Drop(Size beforeSize, Size afterSize);
        void PrintNumber(Size leftSize, Size rightSize);
        void PrintText(Size leftSize, Size itemSize);
        void PrintText(string dumpPrintText);
        void Fiber(FiberHead fiberHead, FiberItem[] fiberItems);
        void List(CodeBase[] data);
        void LocalBlockEnd(Size size, Size intermediateSize);
        void LocalVariableData(Size size, string holder, Size offset, Size dataSize);
        void LocalVariableDefinition(string holderName, Size valueSize);
        void LocalVariableReference(Size size, string holder, Size offset);
        void RecursiveCall();
        void ReferenceCode(IReferenceInCode context);
        void RefPlus(Size size, Size right);
        void ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode);
        void TopData(Size offset, Size size, Size dataSize);
        void TopFrameData(Size offset, Size size, Size dataSize);
        void TopRef(Size offset, Size size);
        void TopFrameRef(Size offset, Size size);
    }

    internal class FormalMaschine : ReniObject, IFormalMaschine
    {
        private readonly Size _startAddress;

        private readonly FormalValueAccess[] _data;
        private FormalValueAccess[] _frameData = new FormalValueAccess[0];
        private readonly FormalPointer[] _points;
        private FormalPointer[] _framePoints = new FormalPointer[1];
        private int _nextValue;
        internal const string Names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

        internal FormalMaschine(Size dataSize)
        {
            _startAddress = dataSize;
            _data = new FormalValueAccess[dataSize.ToInt()];
            _points = new FormalPointer[dataSize.ToInt() + 1];
        }

        internal string CreateGraph()
        {
            return _data.Aggregate("", (current, t) => current + (t == null ? " ?" : t.Dump())) + "  |" +
                   _frameData.Aggregate("", (current, t) => current + (t == null ? " ?" : t.Dump())) + "\n" +
                   _points.Aggregate("", (current, t) => current + (t == null ? "  " : t.Dump())) + "|" +
                   _framePoints.Aggregate("", (current, t) => current + (t == null ? "  " : t.Dump())) + "\n";
        }

        void IFormalMaschine.BitsArray(Size size, BitsConst data)
        {
            var startAddress = (_startAddress - size).ToInt();
            var element = FormalValueAccess.BitsArray(data);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.TopRef(Size offset, Size size)
        {
            var index = (_startAddress + offset).ToInt();
            FormalPointer.Ensure(_points, index);
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(_points[index], startAddress, size);
        }

        void IFormalMaschine.Call(Size size, int functionIndex, Size argsAndRefsSize)
        {
            var formalSubValues = PullInputValuesFromData(argsAndRefsSize);
            var startAddress = (_startAddress + argsAndRefsSize - size).ToInt();
            var element = FormalValueAccess.Call(formalSubValues, functionIndex);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.BitCast(Size size, Size targetSize, Size significantSize)
        {
            var formalSubValue = GetInputValuesFromData(significantSize).OnlyOne();
            var startAddress = (_startAddress + targetSize - size).ToInt();
            var element = FormalValueAccess.BitCast(formalSubValue, (size - significantSize).ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.PrintNumber(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            ResetInputValuesOfData(leftSize);
        }

        void IFormalMaschine.TopFrameData(Size offset, Size size, Size dataSize)
        {
            AlignFrame(offset);
            var access = GetInputValuesFromFrame(offset, size).OnlyOne() ?? CreateValuesInFrame(size, offset);
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(access, startAddress, dataSize);
        }

        void IFormalMaschine.TopData(Size offset, Size size, Size dataSize)
        {
            var source = GetInputValuesFromData(offset, dataSize).OnlyOne();
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(source, startAddress, dataSize);
        }

        void IFormalMaschine.LocalBlockEnd(Size size, Size intermediateSize)
        {
            var dataSize = (size - intermediateSize).ToInt();
            var accesses = new FormalValueAccess[dataSize];
            for(var i = 0; i < dataSize; i++)
                accesses[i] = _data[i + _startAddress.ToInt()];
            for(var i = 0; i < dataSize; i++)
                _data[i + (_startAddress + intermediateSize).ToInt()] = accesses[i];
        }

        void IFormalMaschine.Drop(Size beforeSize, Size afterSize) { ResetInputValuesOfData(beforeSize - afterSize); }

        void IFormalMaschine.RefPlus(Size size, Size right)
        {
            var formalSubValue = PullInputValuesFromData(size).OnlyOne();
            var startAddress = _startAddress.ToInt();
            var element = FormalValueAccess.RefPlus(formalSubValue, right.ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.Dereference(RefAlignParam refAlignParam, Size size, Size dataSize)
        {
            var formalSubValue = PullInputValuesFromData(refAlignParam.RefSize).OnlyOne();
            var startAddress = (_startAddress + refAlignParam.RefSize - size).ToInt();
            var element = FormalValueAccess.Dereference(formalSubValue);
            SetFormalValues(element, startAddress, dataSize);
        }

        void IFormalMaschine.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var formalLeftSubValue = PullInputValuesFromData(leftSize).OnlyOne();
            var formalRightSubValue = PullInputValuesFromData(leftSize, rightSize).OnlyOne();
            var startAddress = (_startAddress + leftSize + rightSize - size).ToInt();
            var element = FormalValueAccess.BitArrayBinaryOp(opToken.DataFunctionName, formalLeftSubValue, formalRightSubValue);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.Assign(Size targetSize, RefAlignParam refAlignParam) { ResetInputValuesOfData(refAlignParam.RefSize*2); }
        void IFormalMaschine.BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize) { NotImplementedMethod(opToken, size, argSize); }
        void IFormalMaschine.PrintText(string dumpPrintText) { NotImplementedMethod(dumpPrintText); }
        void IFormalMaschine.List(CodeBase[] data) { NotImplementedMethod(data); }
        void IFormalMaschine.Fiber(FiberHead fiberHead, FiberItem[] fiberItems) { NotImplementedMethod(fiberHead, fiberItems); }
        void IFormalMaschine.LocalVariableReference(Size size, string holder, Size offset) { NotImplementedMethod(size, holder, offset); }
        void IFormalMaschine.RecursiveCall() { throw new NotImplementedException(); }
        void IFormalMaschine.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode) { NotImplementedMethod(condSize, thenCode, elseCode); }
        void IFormalMaschine.LocalVariableData(Size size, string holder, Size offset, Size dataSize) { NotImplementedMethod(size, holder, offset); }
        void IFormalMaschine.ReferenceCode(IReferenceInCode context) { NotImplementedMethod(context); }
        void IFormalMaschine.LocalVariableDefinition(string holderName, Size valueSize) { NotImplementedMethod(holderName, valueSize); }
        void IFormalMaschine.TopFrameRef(Size offset, Size size) { NotImplementedMethod(offset, size); }
        void IFormalMaschine.PrintText(Size leftSize, Size itemSize) { NotImplementedMethod(leftSize, itemSize); }

        private IFormalValue CreateValuesInFrame(Size size, Size offset)
        {
            var element = FormalValueAccess.Variable(Names[_nextValue++]);
            var size1 = size.ToInt();
            var start = _frameData.Length + offset.ToInt();
            for(var i = 0; i < size1; i++)
                _frameData[i + start] = new FormalValueAccess(element, i, size1);
            return element;
        }

        private void AlignFrame(Size offset)
        {
            var minSize = -offset.ToInt();
            if(_frameData.Length >= minSize)
                return;

            var frameData = _frameData;
            var framePoints = _framePoints;

            _frameData = new FormalValueAccess[minSize];
            _framePoints = new FormalPointer[minSize + 1];

            var delta = _frameData.Length - frameData.Length;

            for(var i = 0; i < frameData.Length; i++)
                _frameData[i + delta] = frameData[i];
            for(var i = 0; i < framePoints.Length; i++)
                _framePoints[i + delta] = framePoints[i];
        }

        private IFormalValue[] GetInputValuesFromFrame(Size offset, Size size)
        {
            var accesses = new List<FormalValueAccess>();
            var start = _frameData.Length + offset.ToInt();
            for(var i = 0; i < size.ToInt(); i++)
                accesses.Add(_frameData[i + start]);
            return FormalValueAccess.Transpose(accesses);
        }

        private IFormalValue[] PullInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (_startAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
            {
                accesses.Add(_data[i + start]);
                _data[i + start] = null;
            }
            return FormalValueAccess.Transpose(accesses);
        }

        private IFormalValue[] GetInputValuesFromData(Size inputSize) { return GetInputValuesFromData(Size.Zero, inputSize); }

        private IFormalValue[] GetInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (_startAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                accesses.Add(_data[i + start]);
            return FormalValueAccess.Transpose(accesses);
        }

        private IFormalValue[] PullInputValuesFromData(Size inputSize) { return PullInputValuesFromData(Size.Zero, inputSize); }

        private void ResetInputValuesOfData(Size inputSize)
        {
            var start = _startAddress.ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                _data[i + start] = null;
        }

        private void SetFormalValues(IFormalValue element, int startAddress, Size size)
        {
            var size1 = size.ToInt();
            for(var i = 0; i < size1; i++)
                _data[i + startAddress] = new FormalValueAccess(element, i, size1);
        }
    }
}