using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Runtime;

namespace Reni.Code
{
    internal sealed class LocalStackReference : ReniObject, IStackDataAddressBase
    {
        [EnableDump]
        private readonly DictionaryEx<string, StackData> _locals;
        [EnableDump]
        private readonly string _holder;

        public LocalStackReference(DictionaryEx<string, StackData> locals, string holder)
        {
            _locals = locals;
            _holder = holder;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _locals[_holder].DoPull(offset).DoGetTop(size); }
        void IStackDataAddressBase.SetTop(Size offset, StackData right)
        {
            var data = ((IStackDataAddressBase)this).GetTop(offset, right.Size);
            var trace = data.DebuggerDumpString;
            NotImplementedMethod(offset, right);
        }

        string IStackDataAddressBase.Dump() { return _holder; }
    }
}