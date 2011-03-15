using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni
{
    internal class Intervall<T> : ReniObject
    {
        public readonly T Start;
        public readonly T End;

        public Intervall(T start, T end)
        {
            Start = start;
            End = end;
        }
    }
}