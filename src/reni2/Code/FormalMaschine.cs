using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    internal interface IFormalMaschine
    {
        void BitsArray(Size size, BitsConst data);
        void TopRef(RefAlignParam refAlignParam, Size offset);
        void Call(Size size, int functionIndex, Size argsAndRefsSize);
        void BitCast(Size size, Size targetSize, Size significantSize);
        void DumpPrintOperation(Size leftSize, Size rightSize);
        void TopFrame(Size size, Size offset);
        void TopData(Size size, Size offset);
        void LocalBlockEnd(Size size, Size intermediateSize);
        void Drop(Size beforeSize, Size afterSize);
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
        private const string Names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

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
            if(_points[index] == null)
                _points[index] = new FormalPointer(Names[_nextPointer++]);

            var size = refAlignParam.RefSize;
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(_points[index], startAddress, size);
        }

        void IFormalMaschine.Call(Size size, int functionIndex, Size argsAndRefsSize)
        {
            var formalSubValue = PullInputValuesFromData(argsAndRefsSize);
            var startAddress = (_startAddress + argsAndRefsSize - size).ToInt();
            var element = FormalValueAccess.Call(formalSubValue, functionIndex);
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.BitCast(Size size, Size targetSize, Size significantSize)
        {
            var formalSubValue = PullInputValuesFromData(targetSize);
            var startAddress = (_startAddress + targetSize - size).ToInt();
            var element = FormalValueAccess.BitCast(formalSubValue, (size - significantSize).ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IFormalMaschine.DumpPrintOperation(Size leftSize, Size rightSize)
        {
            Tracer.Assert(rightSize.IsZero);
            ResetInputValuesOfData(leftSize);
        }

        void IFormalMaschine.TopFrame(Size size, Size offset)
        {
            AlignFrame(offset);
            var access = GetInputValuesFromFrame(size, offset);
            if (access == null)
                access = CreateValuesInFrame(size, offset);
            var startAddress = (_startAddress - size).ToInt();
            SetFormalValues(access, startAddress, size);
        }

        void IFormalMaschine.TopData(Size size, Size offset)
        {
            var startAddress = (_startAddress - size).ToInt();
            var sourceAddress = (_startAddress + offset).ToInt();
            var size1 = size.ToInt();
            for(var i = 0; i < size1; i++)
                _data[i + startAddress] = _data[i + sourceAddress];
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

        private IFormalValue GetInputValuesFromFrame(Size size, Size offset)
        {
            var accesses = new List<FormalValueAccess>();
            var start = _frameData.Length + offset.ToInt();
            for(var i = 0; i < size.ToInt(); i++)
                accesses.Add(_frameData[i + start]);
            return FormalValueAccess.Transpose(accesses);
        }
        private IFormalValue PullInputValuesFromData(Size inputSize)
        {
            var argsAndRefs = new List<FormalValueAccess>();
            var start = _startAddress.ToInt();
            for (var i = 0; i < inputSize.ToInt(); i++)
            {
                argsAndRefs.Add(_data[i + start]);
                _data[i + start] = null;
            }
            return FormalValueAccess.Transpose(argsAndRefs);
        }
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

    sealed internal class FormalPointer : IFormalValue
    {
        private readonly char _name;
        public FormalPointer(char name) { _name = name; }
        string IFormalValue.Dump() { return _name + ""; }
        public string Dump() { return _name + " "; }

        string IFormalValue.Dump(int index, int size)
        {
            var text = CreateText(_name, size);
            return text.Substring(index*2, 2);
        }

        private static string CreateText(char name, int size)
        {
            var text = "P" + name;
            var fillCount = 2*size - text.Length - 1;
            if(fillCount >= 0)
                return " " + "<".Repeat(fillCount/2) + text + ">".Repeat(fillCount - fillCount/2);
            return " " + "P".Repeat(2*size - 1);
        }
    }

    sealed internal class CallValue : IFormalValue
    {
        private readonly IFormalValue _formalSubValue;
        private readonly int _functionIndex;

        public CallValue(IFormalValue formalSubValue, int functionIndex)
        {
            _formalSubValue = formalSubValue;
            _functionIndex = functionIndex;
        }

        private string CreateText(int size)
        {
            var text = Dump();
            var fillCount = 2*size - text.Length - 1;
            if(fillCount >= 0)
                return " " + "<".Repeat(fillCount/2) + text + ">".Repeat(fillCount - fillCount/2);
            return " " + "F".Repeat(2*size - 1);
        }

        public string Dump(int index, int size)
        {
            var text = CreateText(size);
            return text.Substring(index*2, 2);
        }

        public string Dump() { return "F" + _functionIndex + "(" + _formalSubValue.Dump() + ")"; }
    }

    internal class VariableValue : IFormalValue
    {
        private readonly char _name;
        public VariableValue(char name) { _name = name; }

        private string CreateText(int size)
        {
            var text = Dump();
            var fillCount = 2 * size - text.Length - 1;
            if (fillCount >= 0)
                return " " + "<".Repeat(fillCount / 2) + text + ">".Repeat(fillCount - fillCount / 2);
            return " " + "V".Repeat(2 * size - 1);
        }

        string IFormalValue.Dump(int index, int size)
        {
            var text = CreateText(size);
            return text.Substring(index*2, 2);
        }

        public string Dump() { return "V"+_name; }
    }

    internal class FormalValueAccess
    {
        private readonly int _index;
        private readonly int _size;
        private readonly IFormalValue _formalValue;
        public int Index { get { return _index; } }
        public int Size { get { return _size; } }
        public IFormalValue FormalValue { get { return _formalValue; } }

        public static IFormalValue Transpose(IEnumerable<FormalValueAccess> accesses)
        {
            var a0 = accesses.Distinct().ToArray();
            if (a0.Length == 1 && a0[0] == null)
                return null;
            var aa = accesses.Select(x => x.FormalValue).Distinct().ToArray();
            if (aa.Length != 1)
                throw new NotImplementedException();
            var ss = accesses.Select(x => x.Size).Distinct().ToArray();
            if (ss.Length != 1 || ss[0] != accesses.ToArray().Length)
                throw new NotImplementedException();
            var ii = accesses.Select((x, i) => i - x.Index).Distinct().ToArray();
            if (ii.Length != 1 || ii[0] != 0)
                throw new NotImplementedException();
            return aa[0];
        }

        public FormalValueAccess(IFormalValue formalValue, int index, int size)
        {
            _formalValue = formalValue;
            _size = size;
            _index = index;
        }

        public string Dump() { return _formalValue.Dump(_index, _size); }

        public static IFormalValue BitsArray(BitsConst data) { return new BitsArrayValue(data); }
        public static IFormalValue Call(IFormalValue formalSubValue, int functionIndex) { return new CallValue(formalSubValue, functionIndex); }
        public static IFormalValue BitCast(IFormalValue formalSubValue, int castedBits) { return new BitCastValue(formalSubValue, castedBits); }
        public static IFormalValue Variable(char name) { return new VariableValue(name); }
    }

    internal class BitCastValue : IFormalValue
    {
        private readonly IFormalValue _formalSubValue;
        private readonly int _castedBits;

        public BitCastValue(IFormalValue formalSubValue, int castedBits)
        {
            _formalSubValue = formalSubValue;
            _castedBits = castedBits;
        }

        string IFormalValue.Dump(int index, int size)
        {
            if(index >= size - _castedBits)
                return " .";
            return _formalSubValue.Dump(index, size - _castedBits);
        }

        string IFormalValue.Dump() { return _formalSubValue.Dump(); }
    }

    internal sealed class BitsArrayValue : IFormalValue
    {
        private readonly BitsConst _data;

        public BitsArrayValue(BitsConst data) { _data = data; }

        string IFormalValue.Dump(int index, int size)
        {
            switch(_data.Access(Size.Create(index)))
            {
                case false:
                    return " 0";
                case true:
                    return " 1";
                default:
                    return " .";
            }
        }

        public string Dump() { return _data.DumpValue(); }
    }

    internal interface IFormalValue
    {
        string Dump(int index, int size);
        string Dump();
    }
}