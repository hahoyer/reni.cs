using System;

namespace Reni.Code
{
    internal sealed class FrameData : ReniObject, IStackDataAddressBase
    {
        private readonly StackData _data;
        
        public FrameData(StackData data) { _data = data; }
        
        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _data.DoPull(_data.Size + offset).DoGetTop(size); }
        string IStackDataAddressBase.Dump() { return "frame{" +  _data.Dump() +"}"; }

        public StackData Data { get { return _data; } }
    }
}