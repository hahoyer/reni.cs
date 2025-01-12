using Reni.Basics;

namespace Reni.Code
{
    sealed class FrameData : DumpableObject, IStackDataAddressBase
    {
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