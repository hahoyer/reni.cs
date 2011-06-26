using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code
{
    internal sealed class StackDataAddress : NonListStackData
    {
        private readonly IStackDataAddressBase _data;
        private readonly Size _refSize;
        private readonly Size _offset;

        public StackDataAddress(IStackDataAddressBase data, Size refSize, Size offset)
        {
            _data = data;
            _refSize = refSize;
            _offset = offset;
        }

        internal IStackDataAddressBase Data { get { return _data; } }
        internal Size Offset { get { return _offset; } }

        internal override Size Size { get { return _refSize; } }

        internal new StackData Dereference(Size size, Size dataSize) { return _data.GetTop(_offset, size).BitCast(dataSize).BitCast(size); }

        internal new void Assign(Size size, StackData right) { _data.SetTop(_offset, right.Dereference(size,size)); }

        internal new StackData RefPlus(Size offset)
        {
            if(offset.IsZero)
                return this;
            return new StackDataAddress(_data, _refSize, offset + _offset);
        }

        protected override StackDataAddress GetAddress() { return this; }

        protected override string Dump(bool isRecursion)
        {
            if(isRecursion)
                throw new NotImplementedException();
            return _data.Dump() + "[" + _offset.ToInt() + "]";
        }
    }

    internal interface IStackDataAddressBase
    {
        StackData GetTop(Size offset, Size size);
        string Dump();
        void SetTop(Size offset, StackData right);
    }
}