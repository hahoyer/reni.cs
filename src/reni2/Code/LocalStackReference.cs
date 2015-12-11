using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class LocalStackReference : DumpableObject, IStackDataAddressBase
    {
        [EnableDump]
        readonly FunctionCache<string, StackData> _locals;
        [EnableDump]
        readonly string _holder;

        public LocalStackReference(FunctionCache<string, StackData> locals, string holder)
        {
            _locals = locals;
            _holder = holder;
        }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size)
            => _locals[_holder].DoPull(offset).DoGetTop(size);

        void IStackDataAddressBase.SetTop(Size offset, StackData right)
        {
            var data = ((IStackDataAddressBase) this).GetTop(offset, right.Size);
            var trace = data.DebuggerDumpString;
            NotImplementedMethod(offset, right);
        }

        string IStackDataAddressBase.Dump() => _holder;
    }
}