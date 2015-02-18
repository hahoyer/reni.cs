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
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    sealed class ListStack : StackData
    {
        [EnableDump]
        readonly NonListStackData[] _data;

        public ListStack(NonListStackData top, NonListStackData formerStackData)
            : base(top.OutStream) { _data = new[] {top, formerStackData}; }

        ListStack(NonListStackData[] data, IOutStream outStream)
            : base(outStream) { _data = data; }

        ListStack(NonListStackData[] data, NonListStackData formerStackData)
            : base(formerStackData.OutStream)
        {
            _data = new NonListStackData[data.Length + 1];
            data.CopyTo(_data, 0);
            _data[data.Length] = formerStackData;
        }

        ListStack(NonListStackData[] data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            _data = new NonListStackData[data.Length + formerStack._data.Length];
            data.CopyTo(_data, 0);
            formerStack._data.CopyTo(_data, data.Length);
        }

        public ListStack(NonListStackData data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            _data = new NonListStackData[formerStack._data.Length + 1];
            _data[0] = data;
            formerStack._data.CopyTo(_data, 1);
        }

        protected override StackData GetTop(Size size)
        {
            if(_data.Length > 0 && _data[0].Size >= size)
                return _data[0].DoGetTop(size);
            var i = Index(size);
            if(i == null)
                return base.GetTop(size);

            return Head(i.Value);
        }

        int? Index(Size size)
        {
            var sizeSoFar = Size.Zero;
            var i = 0;
            for(; i < _data.Length && sizeSoFar < size; i++)
                sizeSoFar += _data[i].Size;
            if(sizeSoFar > size)
                return null;
            return i;
        }

        protected override StackData Pull(Size size)
        {
            var i = Index(size);
            if(i == null)
                return base.Pull(size);
            return Tail(i.Value);
        }

        StackData Head(int value) => SubString(0, value);
        StackData Tail(int value) => SubString(value, _data.Length - value);

        StackData SubString(int start, int length)
        {
            switch(length)
            {
                case 0:
                    return new EmptyStackData(OutStream);
                case 1:
                    return _data[start];
                default:
                {
                    var newData = new NonListStackData[length];
                    for(var i = 0; i < length; i++)
                        newData[i] = _data[start + i];
                    return new ListStack(newData, OutStream);
                }
            }
        }

        internal override StackData PushOnto(NonListStackData formerStack) => new ListStack(_data, formerStack);
        internal override StackData PushOnto(ListStack formerStack) => new ListStack(_data, formerStack);
        internal override BitsConst GetBitsConst() => _data.Aggregate(BitsConst.None(), Paste);
        static BitsConst Paste(BitsConst bitsConst, NonListStackData data) => bitsConst.Concat(data.GetBitsConst());
        internal override StackData Push(StackData formerStack) => formerStack.PushOnto(this);
        internal override Size Size { get { return _data.Aggregate(Size.Zero, (current, data) => current + data.Size); } }
    }

    abstract class NonListStackData : StackData
    {
        protected NonListStackData(IOutStream outStream)
            : base(outStream) { }
        internal override StackData Push(StackData stackData) => stackData.PushOnto(this);
        internal override StackData PushOnto(NonListStackData formerStack) => new ListStack(this, formerStack);
        internal override StackData PushOnto(ListStack formerStack) => new ListStack(this, formerStack);
    }
}