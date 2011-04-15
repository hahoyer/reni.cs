﻿using System;
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
        protected override StackData GetTop(Size size)
        {
            if (size < Size)
                return BitCast(size);
            return base.GetTop(size);
        }
        internal override Size Size { get { return GetBitsConst().Size; } }
    }
}