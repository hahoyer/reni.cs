using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    internal interface IFormalMaschine
    {
        void BitsArray(Size size, BitsConst data);
        void TopRef(RefAlignParam refAlignParam, Size offset);
        void Call(Size size, int functionIndex, Size argsAndRefsSize);
        void BitCast(Size size, Size targetSize, Size significantSize);
        void DumpPrintOperation(Size leftSize, Size rightSize);
        void TopFrame(Size offset, Size size, Size dataSize);
        void TopData(Size offset, Size size, Size dataSize);
        void LocalBlockEnd(Size size, Size intermediateSize);
        void Drop(Size beforeSize, Size afterSize);
        void RefPlus(Size size, Size right);
        void Dereference(RefAlignParam refAlignParam, Size size, Size dataSize);
        void BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize);
        void Assign(Size targetSize, RefAlignParam refAlignParam);
        void DumpPrintText();
    }

    internal class FormalMaschine : ReniObject, IFormalMaschine
    {
        private Size _startAddress;

        private readonly FormalValueAccess[] _data;
        private FormalValueAccess[] _frameData = new FormalValueAccess[0];
        private readonly FormalPointer[] _points;
        private FormalPointer[] _framePoints = new FormalPointer[1];
        private int _nextPointer;
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

        internal void ShiftStartAddress(Size deltaSize)
        {
            _startAddress += deltaSize;
            for(var i = 0; i < _startAddress.ToInt(); i++)
                _data[i] = null;
        }

        void IFormalMaschine.BitsArray(Size size, BitsConst data)
        {
            var startAddress = (_startAddress - size).ToInt();
            var element = FormalValueAccess.BitsArray(data);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.TopRef(RefAlignParam refAlignParam, Size offset)
        {
            var index = (_startAddress + offset).ToInt();
            FormalPointer.Ensure(_points, index);
            var size = refAlignParam.RefSize;
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
            var formalSubValue = PullInputValuesFromData(significantSize).OnlyOne();
            var startAddress = (_startAddress + targetSize - size).ToInt();
            var element = FormalValueAccess.BitCast(formalSubValue, (size - significantSize).ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.DumpPrintOperation(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            ResetInputValuesOfData(leftSize);
        }

        void IFormalMaschine.TopFrame(Size offset, Size size, Size dataSize)
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

        void IFormalMaschine.Drop(Size beforeSize, Size afterSize)
        {
            ResetInputValuesOfData(beforeSize - afterSize);
        }

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
            var startAddress = (_startAddress+ refAlignParam.RefSize- size).ToInt();
            var element = FormalValueAccess.Dereference(formalSubValue);
            SetFormalValues(element, startAddress, dataSize);
        }

        void IFormalMaschine.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var formalLeftSubValue = PullInputValuesFromData(leftSize).OnlyOne();
            var formalRightSubValue = PullInputValuesFromData(leftSize, rightSize).OnlyOne();
            var startAddress = (_startAddress + leftSize+rightSize - size).ToInt();
            var element = FormalValueAccess.BitArrayBinaryOp(opToken.DataFunctionName, formalLeftSubValue,formalRightSubValue);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.Assign(Size targetSize, RefAlignParam refAlignParam)
        {
            ResetInputValuesOfData(refAlignParam.RefSize*2);
        }

        void IFormalMaschine.DumpPrintText() { }

        private IFormalValue CreateValuesInFrame(Size size, Size offset)
        {
            IFormalValue element= FormalValueAccess.Variable(Names[_nextValue++]);
            var size1 = size.ToInt();
            var start = _frameData.Length + offset.ToInt();
            for (var i = 0; i < size1; i++)
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

            for(int i = 0; i < frameData.Length; i++)
                _frameData[i + delta] = frameData[i];
            for (int i = 0; i < framePoints.Length; i++)
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
            var start = (_startAddress+offset).ToInt();
            for (var i = 0; i < inputSize.ToInt(); i++)
            {
                accesses.Add(_data[i + start]);
                _data[i + start] = null;
            }
            return FormalValueAccess.Transpose(accesses);
        }

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
            for (var i = 0; i < size1; i++)
                _data[i + startAddress] = new FormalValueAccess(element, i, size1);
        }

    }

}