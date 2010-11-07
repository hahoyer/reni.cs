using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableAccess : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly string _holder;
        private readonly Size _offset;
        private readonly Size _size;
        private readonly Size _dataSize;

        public LocalVariableAccess(RefAlignParam refAlignParam, string holder, Size offset, Size size, Size dataSize)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
            _offset = offset;
            _size = size;
            _dataSize = dataSize;
        }

        protected override Size GetSize() { return _size; }
        protected override string CSharpString() { return CSharpGenerator.LocalVariableAccess(_holder,_offset, _size); }
    }
}