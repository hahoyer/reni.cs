#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;

namespace Reni.Code
{
    [UsedImplicitly]
    sealed class FormalMachine : DumpableObject, IVisitor
    {
        readonly Size _startAddress;

        readonly FormalValueAccess[] _data;
        FormalValueAccess[] _frameData = new FormalValueAccess[0];
        readonly FormalPointer[] _points;
        FormalPointer[] _framePoints = new FormalPointer[1];
        int _nextValue;
        internal const string Names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        static Size RefSize { get { return Root.DefaultRefAlignParam.RefSize; } }

        internal FormalMachine(Size dataSize)
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

        void IVisitor.BitsArray(Size size, BitsConst data)
        {
            var startAddress = (_startAddress - size).ToInt();
            var element = FormalValueAccess.BitsArray(data);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.TopRef(Size offset)
        {
            var index = (_startAddress + offset).ToInt();
            FormalPointer.Ensure(_points, index);
            var startAddress = (_startAddress - RefSize).ToInt();
            SetFormalValues(_points[index], startAddress, RefSize);
        }

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        {
            var formalSubValues = PullInputValuesFromData(argsAndRefsSize);
            var startAddress = (_startAddress + argsAndRefsSize - size).ToInt();
            var element = FormalValueAccess.Call(formalSubValues, functionId);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
        {
            var formalSubValue = GetInputValuesFromData(significantSize).Single();
            var startAddress = (_startAddress + targetSize - size).ToInt();
            var element = FormalValueAccess.BitCast(formalSubValue, (size - significantSize).ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.PrintNumber(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            ResetInputValuesOfData(leftSize);
        }

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        {
            AlignFrame(offset);
            var access = GetInputValuesFromFrame(offset, size).Single() ?? CreateValuesInFrame(size, offset);
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(access, startAddress, dataSize);
        }

        void IVisitor.TopData(Size offset, Size size, Size dataSize)
        {
            var source = GetInputValuesFromData(offset, dataSize).Single();
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(source, startAddress, dataSize);
        }

        void IVisitor.LocalBlockEnd(Size size, Size intermediateSize)
        {
            var dataSize = (size - intermediateSize).ToInt();
            var accesses = new FormalValueAccess[dataSize];
            for(var i = 0; i < dataSize; i++)
                accesses[i] = _data[i + _startAddress.ToInt()];
            for(var i = 0; i < dataSize; i++)
                _data[i + (_startAddress + intermediateSize).ToInt()] = accesses[i];
        }

        void IVisitor.Drop(Size beforeSize, Size afterSize) { ResetInputValuesOfData(beforeSize - afterSize); }

        void IVisitor.ReferencePlus(Size right)
        {
            var formalSubValue = PullInputValuesFromData(RefSize).Single();
            var startAddress = _startAddress.ToInt();
            var element = FormalValueAccess.RefPlus(formalSubValue, right.ToInt());
            SetFormalValues(element, startAddress, RefSize);
        }

        void IVisitor.DePointer(Size size, Size dataSize)
        {
            var formalSubValue = PullInputValuesFromData(RefSize).Single();
            var startAddress = (_startAddress + RefSize - size).ToInt();
            var element = FormalValueAccess.Dereference(formalSubValue);
            SetFormalValues(element, startAddress, dataSize);
        }

        void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
        {
            var formalLeftSubValue = PullInputValuesFromData(leftSize).Single();
            var formalRightSubValue = PullInputValuesFromData(leftSize, rightSize).Single();
            var startAddress = (_startAddress + leftSize + rightSize - size).ToInt();
            var element = FormalValueAccess.BitArrayBinaryOp(opToken, formalLeftSubValue, formalRightSubValue);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.Assign(Size targetSize) { ResetInputValuesOfData(RefSize * 2); }
        void IVisitor.BitArrayPrefixOp(string operation, Size size, Size argSize) { NotImplementedMethod(operation, size, argSize); }
        void IVisitor.PrintText(string dumpPrintText) { NotImplementedMethod(dumpPrintText); }
        void IVisitor.List(CodeBase[] data) { NotImplementedMethod(data); }
        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems) { NotImplementedMethod(fiberHead, fiberItems); }
        void IVisitor.LocalVariableReference(string holder, Size offset) { NotImplementedMethod(holder, offset); }
        void IVisitor.RecursiveCall() { throw new NotImplementedException(); }
        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode) { NotImplementedMethod(condSize, thenCode, elseCode); }
        void IVisitor.LocalVariableAccess(string holder, Size offset, Size size, Size dataSize) { NotImplementedMethod(size, holder, offset); }
        void IVisitor.ReferenceCode(IContextReference context) { NotImplementedMethod(context); }
        void IVisitor.LocalVariableDefinition(string holderName, Size valueSize) { NotImplementedMethod(holderName, valueSize); }
        void IVisitor.TopFrameRef(Size offset) { NotImplementedMethod(offset); }
        void IVisitor.RecursiveCallCandidate() { throw new NotImplementedException(); }
        void IVisitor.PrintText(Size leftSize, Size itemSize) { NotImplementedMethod(leftSize, itemSize); }

        IFormalValue CreateValuesInFrame(Size size, Size offset)
        {
            var element = FormalValueAccess.Variable(Names[_nextValue++]);
            var size1 = size.ToInt();
            var start = _frameData.Length + offset.ToInt();
            for(var i = 0; i < size1; i++)
                _frameData[i + start] = new FormalValueAccess(element, i, size1);
            return element;
        }

        void AlignFrame(Size offset)
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

        IFormalValue[] GetInputValuesFromFrame(Size offset, Size size)
        {
            var accesses = new List<FormalValueAccess>();
            var start = _frameData.Length + offset.ToInt();
            for(var i = 0; i < size.ToInt(); i++)
                accesses.Add(_frameData[i + start]);
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] PullInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (_startAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
            {
                accesses.Add(_data[i + start]);
                _data[i + start] = null;
            }
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] GetInputValuesFromData(Size inputSize) { return GetInputValuesFromData(Size.Zero, inputSize); }

        IFormalValue[] GetInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (_startAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                accesses.Add(_data[i + start]);
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] PullInputValuesFromData(Size inputSize) { return PullInputValuesFromData(Size.Zero, inputSize); }

        void ResetInputValuesOfData(Size inputSize)
        {
            var start = _startAddress.ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                _data[i + start] = null;
        }

        void SetFormalValues(IFormalValue element, int startAddress, Size size)
        {
            var size1 = size.ToInt();
            for(var i = 0; i < size1; i++)
                _data[i + startAddress] = new FormalValueAccess(element, i, size1);
        }
    }
}