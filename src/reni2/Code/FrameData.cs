using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    internal sealed class FrameData : ReniObject, IStackDataAddressBase
    {
        private readonly StackData _data;
        internal readonly DictionaryEx<string, StackData> Locals = new DictionaryEx<string, StackData>();
        internal bool IsRepeatRequired = false;

        public FrameData(StackData data) { _data = data; }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _data.DoPull(_data.Size + offset).DoGetTop(size); }
        void IStackDataAddressBase.SetTop(Size offset, StackData right)
        {
            NotImplementedMethod(offset, right);
        }
        string IStackDataAddressBase.Dump() { return "frame{" + _data.Dump() + "}"; }

        public StackData Data { get { return _data; } }
    }
}