using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;

namespace Reni.Code
{
    internal class FormalValueAccess
    {
        private readonly int _index;
        private readonly int _size;
        private readonly IFormalValue _formalValue;
        internal int Index { get { return _index; } }
        internal int Size { get { return _size; } }
        private IFormalValue FormalValue { get { return _formalValue; } }

        public static IFormalValue[] Transpose(IEnumerable<FormalValueAccess> accesses)
        {
            var distinctAccesses = accesses.Distinct().ToArray();
            if(distinctAccesses.Length == 1 && distinctAccesses[0] == null)
                return new IFormalValue[1];
            var result = accesses.Select(x => x == null ? null : x.FormalValue).Distinct().ToArray();
            foreach(var formalValue in result)
            {
                if(formalValue != null)
                    formalValue.Check(accesses.Where(x => x != null && x.FormalValue == formalValue));
            }
            return result;
        }

        public FormalValueAccess(IFormalValue formalValue, int index, int size)
        {
            _formalValue = formalValue;
            _size = size;
            _index = index;
        }

        public string Dump() { return _formalValue.Dump(_index, _size); }

        public static IFormalValue BitsArray(BitsConst data) { return new BitsArrayValue(data); }
        public static IFormalValue Call(IFormalValue[] formalSubValues, int functionIndex) { return new CallValue(formalSubValues, functionIndex); }
        public static IFormalValue BitCast(IFormalValue formalSubValue, int castedBits) { return new BitCastValue(formalSubValue, castedBits); }
        public static IFormalValue Variable(char name) { return new VariableValue(name); }
        public static IFormalValue RefPlus(IFormalValue formalSubValue, int right) { return formalSubValue.RefPlus(right); }
        public static IFormalValue Dereference(IFormalValue formalSubValue) { return new DereferenceValue(formalSubValue); }
        public static IFormalValue BitArrayBinaryOp(string operation, IFormalValue formalLeftSubValue, IFormalValue formalRightSubValue) { return new BitArrayBinaryOpValue(operation, formalLeftSubValue, formalRightSubValue); }

        internal static void Check(IEnumerable<FormalValueAccess> accesses)
        {
            var ss = accesses.Select(x => x.Size).Distinct().ToArray();
            if(ss.Length != 1 || ss[0] != accesses.ToArray().Length)
                Tracer.FlaggedLine("Size problem");
            var ii = accesses.Select((x, i) => i - x.Index).Distinct().ToArray();
            if(ii.Length != 1 || ii[0] != 0)
                Tracer.FlaggedLine("Consequtivity problem");
        }
    }

    internal class BitArrayBinaryOpValue : NamedValue
    {
        private readonly string _operation;
        private readonly IFormalValue _leftSubValue;
        private readonly IFormalValue _rightSubValue;

        public BitArrayBinaryOpValue(string operation, IFormalValue leftSubValue, IFormalValue rightSubValue)
        {
            _operation = operation;
            _leftSubValue = leftSubValue;
            _rightSubValue = rightSubValue;
        }

        protected override char DumpShort() { return _operation[0]; }

        protected override string Dump(bool isRecursion) { return "(" + _leftSubValue + ")" + _operation + "(" + _rightSubValue + ")"; }
    }

    internal class DereferenceValue : NamedValue
    {
        private readonly IFormalValue _formalSubValue;
        public DereferenceValue(IFormalValue formalSubValue) { _formalSubValue = formalSubValue; }
        protected override char DumpShort() { return 'd'; }

        protected override string Dump(bool isRecursion) { return "(" + _formalSubValue.Dump() + ")d"; }
    }

    internal sealed class FormalPointer : NamedValue
    {
        [EnableDump]
        private readonly char _name;

        private readonly FormalPointer[] _points;
        private readonly int _index;
        private static int _nextPointer;

        private FormalPointer(FormalPointer[] points, int index)
        {
            _name = FormalMachine.Names[_nextPointer++];
            _points = points;
            _index = index;
        }

        protected override string Dump(bool isRecursion) { return _name + " "; }

        protected override IFormalValue RefPlus(int right)
        {
            Ensure(_points, _index + right);
            return _points[_index + right];
        }

        protected override char DumpShort() { return _name; }

        public static void Ensure(FormalPointer[] points, int index)
        {
            if(points[index] == null)
                points[index] = new FormalPointer(points, index);
        }
    }

    internal abstract class NamedValue : ReniObject, IFormalValue
    {
        private string CreateText(int size)
        {
            var text = Dump();
            var fillCount = 2*size - text.Length - 1;
            if(fillCount >= 0)
                return " " + "<".Repeat(fillCount/2) + text + ">".Repeat(fillCount - fillCount/2);
            return " " + (DumpShort() + "").Repeat(2*size - 1);
        }

        protected new abstract char DumpShort();

        string IFormalValue.Dump(int index, int size)
        {
            var text = CreateText(size);
            return text.Substring(index*2, 2);
        }

        string IFormalValue.Dump() { return Dump(); }

        protected abstract override string Dump(bool isRecursion);

        IFormalValue IFormalValue.RefPlus(int right) { return RefPlus(right); }

        void IFormalValue.Check(IEnumerable<FormalValueAccess> accesses) { FormalValueAccess.Check(accesses); }

        protected virtual IFormalValue RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }


    internal sealed class CallValue : NamedValue
    {
        private readonly IFormalValue[] _formalSubValues;
        private readonly int _functionIndex;

        public CallValue(IFormalValue[] formalSubValues, int functionIndex)
        {
            _formalSubValues = formalSubValues;
            _functionIndex = functionIndex;
        }

        protected override char DumpShort() { return 'F'; }

        protected override string Dump(bool isRecursion) { return "F" + _functionIndex + "(" + _formalSubValues.Aggregate("", (x, y) => (x == "" ? "" : x + ", ") + y.Dump()) + ")"; }
    }

    internal class VariableValue : NamedValue
    {
        private readonly char _name;
        public VariableValue(char name) { _name = name; }
        private bool _isPointer;

        protected override char DumpShort() { return 'V'; }

        protected override string Dump(bool isRecursion)
        {
            var result = "V" + _name;
            if(_isPointer)
                result += "[p]";
            return result;
        }

        protected override IFormalValue RefPlus(int right)
        {
            _isPointer = true;
            return new PointerValue(this, right);
        }
    }

    internal class PointerValue : NamedValue
    {
        private readonly VariableValue _variableValue;
        private readonly int _right;

        public PointerValue(VariableValue variableValue, int right)
        {
            _variableValue = variableValue;
            _right = right;
        }

        protected override char DumpShort() { return 'P'; }

        protected override string Dump(bool isRecursion) { return _variableValue.Dump() + FormatRight(); }

        private string FormatRight()
        {
            if(_right > 0)
                return "+" + _right;
            if(_right < 0)
                return "-" + -_right;
            return "";
        }
    }

    internal class BitCastValue : ReniObject, IFormalValue
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
        void IFormalValue.Check(IEnumerable<FormalValueAccess> accesses) { FormalValueAccess.Check(accesses); }

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }

    internal sealed class BitsArrayValue : ReniObject, IFormalValue
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

        protected override string Dump(bool isRecursion) { return _data.DumpValue(); }
        void IFormalValue.Check(IEnumerable<FormalValueAccess> accesses) { }

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }
}