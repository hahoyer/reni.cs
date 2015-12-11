using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    internal sealed class BitsStackData : NonListStackData
    {
        [EnableDump]
        private readonly BitsConst _data;

        public BitsStackData(BitsConst data, IOutStream outStream)
            : base(outStream)
        {
            _data = data;
            Tracer.Assert(!_data.Size.IsZero);
        }

        protected override string Dump(bool isRecursion) => _data.DumpValue();

        internal override BitsConst GetBitsConst() => _data;
        protected override StackData GetTop(Size size)
        {
            if (size < Size)
                return BitCast(size);
            return base.GetTop(size);
        }
        internal override Size Size => GetBitsConst().Size;
    }
}