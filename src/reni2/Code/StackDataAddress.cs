using System;

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
        internal override StackData Dereference(Size size) { return _data.GetTop(_offset, size); }

        public override string Dump() { return _data.Dump() + "[" + _offset.ToInt() + "]"; }
    }

    internal interface IStackDataAddressBase
    {
        StackData GetTop(Size offset, Size size);
        string Dump();
    }
}