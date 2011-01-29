using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class BitsStackData : NonListStackData
    {
        [IsDumpEnabled(true)]
        private readonly BitsConst _data;

        public BitsStackData(BitsConst data)
        {
            _data = data;
            Tracer.Assert(!_data.Size.IsZero);
        }

        protected override string Dump(bool isRecursion) { return _data.DumpValue(); }

        internal override BitsConst GetBitsConst() { return _data; }
        internal override Size Size { get { return GetBitsConst().Size; } }
    }
}