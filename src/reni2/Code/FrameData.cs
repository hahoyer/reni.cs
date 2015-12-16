using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class FrameData : DumpableObject, IStackDataAddressBase
    {
        [DisableDump]
        internal readonly FunctionCache<string, StackData> Locals =
            new FunctionCache<string, StackData>();

        [EnableDumpExcept(false)]
        internal bool IsRepeatRequired;

        public FrameData(StackData data) { Data = data; }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size)
            => Data.DoPull(Data.Size + offset).DoGetTop(size);

        void IStackDataAddressBase.SetTop(Size offset, StackData right)
            => NotImplementedMethod(offset, right);

        string IStackDataAddressBase.Dump() => "frame{" + Data.Dump() + "}";

        [EnableDumpExcept(null)]
        public StackData Data { get; }
    }
}