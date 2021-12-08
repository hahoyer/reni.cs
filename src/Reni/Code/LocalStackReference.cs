using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class LocalStackReference : DumpableObject, IStackDataAddressBase
    {
        [EnableDump]
        readonly FunctionCache<string, StackData> Locals;

        [EnableDump]
        readonly string Holder;

        public LocalStackReference(FunctionCache<string, StackData> locals, string holder)
        {
            Locals = locals;
            Holder = holder;
        }

        string IStackDataAddressBase.Dump() => Holder;

        StackData IStackDataAddressBase.GetTop(Size offset, Size size)
            => Locals[Holder].DoPull(offset).DoGetTop(size);

        void IStackDataAddressBase.SetTop(Size offset, StackData right)
        {
            var dataForTrace = ((IStackDataAddressBase)this).GetTop(offset, right.Size);
            NotImplementedMethod(offset, right, nameof(dataForTrace), dataForTrace);
        }
    }
}