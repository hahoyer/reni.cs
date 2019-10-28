using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni
{
    abstract class Singleton<T, TBase> : DumpableObject
        where T : TBase, new()
    {
        internal static readonly T Instance = new T();

        protected Singleton(int? objectId = null)
            : base(objectId) { }
    }
}