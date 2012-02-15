// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class FormalValueAccess : ReniObject
    {
        readonly int _index;
        readonly int _size;
        readonly IFormalValue _formalValue;
        int Index { get { return _index; } }
        int Size { get { return _size; } }
        IFormalValue FormalValue { get { return _formalValue; } }

        public static IFormalValue[] Transpose(FormalValueAccess[] accesses)
        {
            var distinctAccesses = accesses.Distinct().ToArray();
            if(distinctAccesses.Length == 1 && distinctAccesses[0] == null)
                return new IFormalValue[1];
            var result = accesses.Select(x => x == null ? null : x.FormalValue).Distinct().ToArray();
            foreach(var formalValue in result)
                if(formalValue != null)
                    formalValue.Check(accesses.Where(x => x != null && x.FormalValue == formalValue).ToArray());
            return result;
        }

        public FormalValueAccess(IFormalValue formalValue, int index, int size)
        {
            _formalValue = formalValue;
            _size = size;
            _index = index;
        }

        public new string Dump() { return _formalValue.Dump(_index, _size); }

        public static IFormalValue BitsArray(BitsConst data) { return new BitsArrayValue(data); }
        public static IFormalValue Call(IFormalValue[] formalSubValues, int functionIndex) { return new CallValue(formalSubValues, functionIndex); }
        public static IFormalValue BitCast(IFormalValue formalSubValue, int castedBits) { return new BitCastValue(formalSubValue, castedBits); }
        public static IFormalValue Variable(char name) { return new VariableValue(name); }
        public static IFormalValue RefPlus(IFormalValue formalSubValue, int right) { return formalSubValue.RefPlus(right); }
        public static IFormalValue Dereference(IFormalValue formalSubValue) { return new DereferenceValue(formalSubValue); }
        public static IFormalValue BitArrayBinaryOp(string operation, IFormalValue formalLeftSubValue, IFormalValue formalRightSubValue) { return new BitArrayBinaryOpValue(operation, formalLeftSubValue, formalRightSubValue); }

        internal static void Check(FormalValueAccess[] accesses)
        {
            var ss = accesses.Select(x => x.Size).Distinct().ToArray();
            if(ss.Length != 1 || ss[0] != accesses.ToArray().Length)
                Tracer.FlaggedLine("Size problem");
            var ii = accesses.Select((x, i) => i - x.Index).Distinct().ToArray();
            if(ii.Length != 1 || ii[0] != 0)
                Tracer.FlaggedLine("Consequtivity problem");
        }
    }

    sealed class BitArrayBinaryOpValue : NamedValue
    {
        readonly string _operation;
        readonly IFormalValue _leftSubValue;
        readonly IFormalValue _rightSubValue;

        public BitArrayBinaryOpValue(string operation, IFormalValue leftSubValue, IFormalValue rightSubValue)
        {
            _operation = operation;
            _leftSubValue = leftSubValue;
            _rightSubValue = rightSubValue;
        }

        protected override char DumpShort() { return _operation[0]; }

        protected override string Dump(bool isRecursion) { return "(" + _leftSubValue + ")" + _operation + "(" + _rightSubValue + ")"; }
    }

    sealed class DereferenceValue : NamedValue
    {
        readonly IFormalValue _formalSubValue;
        public DereferenceValue(IFormalValue formalSubValue) { _formalSubValue = formalSubValue; }
        protected override char DumpShort() { return 'd'; }

        protected override string Dump(bool isRecursion) { return "(" + _formalSubValue.Dump() + ")d"; }
    }

    sealed class FormalPointer : NamedValue
    {
        [EnableDump]
        readonly char _name;

        readonly FormalPointer[] _points;
        readonly int _index;
        static int _nextPointer;

        FormalPointer(FormalPointer[] points, int index)
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

    abstract class NamedValue : ReniObject, IFormalValue
    {
        string CreateText(int size)
        {
            var text = Dump();
            var fillCount = 2 * size - text.Length - 1;
            if(fillCount >= 0)
                return " " + "<".Repeat(fillCount / 2) + text + ">".Repeat(fillCount - fillCount / 2);
            return " " + (DumpShort() + "").Repeat(2 * size - 1);
        }

        protected new abstract char DumpShort();

        string IFormalValue.Dump(int index, int size)
        {
            var text = CreateText(size);
            return text.Substring(index * 2, 2);
        }

        string IFormalValue.Dump() { return Dump(); }

        protected abstract override string Dump(bool isRecursion);

        IFormalValue IFormalValue.RefPlus(int right) { return RefPlus(right); }

        void IFormalValue.Check(FormalValueAccess[] accesses) { FormalValueAccess.Check(accesses); }

        protected virtual IFormalValue RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }


    sealed class CallValue : NamedValue
    {
        readonly IFormalValue[] _formalSubValues;
        readonly int _functionIndex;

        public CallValue(IFormalValue[] formalSubValues, int functionIndex)
        {
            _formalSubValues = formalSubValues;
            _functionIndex = functionIndex;
        }

        protected override char DumpShort() { return 'F'; }

        protected override string Dump(bool isRecursion) { return "F" + _functionIndex + "(" + _formalSubValues.Aggregate("", (x, y) => (x == "" ? "" : x + ", ") + y.Dump()) + ")"; }
    }

    sealed class VariableValue : NamedValue
    {
        readonly char _name;
        public VariableValue(char name) { _name = name; }
        bool _isPointer;

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

    sealed class PointerValue : NamedValue
    {
        readonly VariableValue _variableValue;
        readonly int _right;

        public PointerValue(VariableValue variableValue, int right)
        {
            _variableValue = variableValue;
            _right = right;
        }

        protected override char DumpShort() { return 'P'; }

        protected override string Dump(bool isRecursion) { return _variableValue.Dump() + FormatRight(); }

        string FormatRight()
        {
            if(_right > 0)
                return "+" + _right;
            if(_right < 0)
                return "-" + -_right;
            return "";
        }
    }

    sealed class BitCastValue : ReniObject, IFormalValue
    {
        readonly IFormalValue _formalSubValue;
        readonly int _castedBits;

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
        void IFormalValue.Check(FormalValueAccess[] accesses) { FormalValueAccess.Check(accesses); }

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }

    sealed class BitsArrayValue : ReniObject, IFormalValue
    {
        readonly BitsConst _data;

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
        void IFormalValue.Check(FormalValueAccess[] formalValueAccesses) { }

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }
}