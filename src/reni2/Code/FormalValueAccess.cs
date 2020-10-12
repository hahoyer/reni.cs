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

using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Struct;

namespace Reni.Code
{
    sealed class FormalValueAccess : DumpableObject
    {
        readonly int _index;
        readonly int _size;
        readonly IFormalValue _formalValue;
        int Index => _index;
        int Size => _size;
        IFormalValue FormalValue => _formalValue;

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

        public new string Dump() => _formalValue.Dump(_index, _size);

        public static IFormalValue BitsArray(BitsConst data) => new BitsArrayValue(data);
        public static IFormalValue Call(IFormalValue[] formalSubValues, FunctionId functionId) => new CallValue(formalSubValues, functionId);
        public static IFormalValue BitCast(IFormalValue formalSubValue, int castedBits) => new BitCastValue(formalSubValue, castedBits);
        public static IFormalValue Variable(char name) => new VariableValue(name);
        public static IFormalValue RefPlus(IFormalValue formalSubValue, int right) => formalSubValue.RefPlus(right);
        public static IFormalValue Dereference(IFormalValue formalSubValue) => new DereferenceValue(formalSubValue);
        public static IFormalValue BitArrayBinaryOp(string operation, IFormalValue formalLeftSubValue, IFormalValue formalRightSubValue) => new BitArrayBinaryOpValue(operation, formalLeftSubValue, formalRightSubValue);

        internal static void Check(FormalValueAccess[] accesses)
        {
            var ss = accesses.Select(x => x.Size).Distinct().ToArray();
            if(ss.Length != 1 || ss[0] != accesses.ToArray().Length)
                "Size problem".FlaggedLine();
            var ii = accesses.Select((x, i) => i - x.Index).Distinct().ToArray();
            if(ii.Length != 1 || ii[0] != 0)
                "Consequtivity problem".FlaggedLine();
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

        protected override char GetNodeDump() => _operation[0];

        protected override string Dump(bool isRecursion) => "(" + _leftSubValue + ")" + _operation + "(" + _rightSubValue + ")";
    }

    sealed class DereferenceValue : NamedValue
    {
        readonly IFormalValue _formalSubValue;
        public DereferenceValue(IFormalValue formalSubValue) { _formalSubValue = formalSubValue; }
        protected override char GetNodeDump() => 'd';

        protected override string Dump(bool isRecursion) => "(" + _formalSubValue.Dump() + ")d";
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

        protected override string Dump(bool isRecursion) => _name + " ";

        protected override IFormalValue RefPlus(int right)
        {
            Ensure(_points, _index + right);
            return _points[_index + right];
        }

        protected override char GetNodeDump() => _name;

        public static void Ensure(FormalPointer[] points, int index)
        {
            if(points[index] == null)
                points[index] = new FormalPointer(points, index);
        }
    }

    abstract class NamedValue : DumpableObject, IFormalValue
    {
        string CreateText(int size)
        {
            var text = Dump();
            var fillCount = 2 * size - text.Length - 1;
            if(fillCount >= 0)
                return " " + "<".Repeat(fillCount / 2) + text + ">".Repeat(fillCount - fillCount / 2);
            return " " + (GetNodeDump() + "").Repeat(2 * size - 1);
        }

        protected new abstract char GetNodeDump();

        string IFormalValue.Dump(int index, int size)
        {
            var text = CreateText(size);
            return text.Substring(index * 2, 2);
        }

        string IFormalValue.Dump() => Dump();

        protected abstract override string Dump(bool isRecursion);

        IFormalValue IFormalValue.RefPlus(int right) => RefPlus(right);

        void IFormalValue.Check(FormalValueAccess[] accesses) => FormalValueAccess.Check(accesses);

        protected virtual IFormalValue RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }


    sealed class CallValue : NamedValue
    {
        readonly IFormalValue[] _formalSubValues;
        readonly FunctionId _functionId;

        public CallValue(IFormalValue[] formalSubValues, FunctionId functionId)
        {
            _formalSubValues = formalSubValues;
            _functionId = functionId;
        }

        protected override char GetNodeDump() => 'F';

        protected override string Dump(bool isRecursion) { return "F" + _functionId + "(" + _formalSubValues.Aggregate("", (x, y) => (x == "" ? "" : x + ", ") + y.Dump()) + ")"; }
    }

    sealed class VariableValue : NamedValue
    {
        readonly char _name;
        public VariableValue(char name) { _name = name; }
        bool _isPointer;

        protected override char GetNodeDump() => 'V';

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

        protected override char GetNodeDump() => 'P';

        protected override string Dump(bool isRecursion) => _variableValue.Dump() + FormatRight();

        string FormatRight()
        {
            if(_right > 0)
                return "+" + _right;
            if(_right < 0)
                return "-" + -_right;
            return "";
        }
    }

    sealed class BitCastValue : DumpableObject, IFormalValue
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

        string IFormalValue.Dump() => _formalSubValue.Dump();
        void IFormalValue.Check(FormalValueAccess[] accesses) => FormalValueAccess.Check(accesses);

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }

    sealed class BitsArrayValue : DumpableObject, IFormalValue
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

        protected override string Dump(bool isRecursion) => _data.DumpValue();
        void IFormalValue.Check(FormalValueAccess[] formalValueAccesses) { }

        IFormalValue IFormalValue.RefPlus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }
}