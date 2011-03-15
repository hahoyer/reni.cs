using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    internal sealed class LocalStackReference : ReniObject, IStackDataAddressBase
    {
        private readonly DictionaryEx<string, StackData> _locals;
        private readonly string _holder;

        public LocalStackReference(DictionaryEx<string, StackData> locals, string holder)
        {
            _locals = locals;
            _holder = holder;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _locals[_holder].DoPull(offset).DoGetTop(size); }

        string IStackDataAddressBase.Dump() { return _holder; }
    }
}