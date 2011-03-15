using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal interface IFormalValue
    {
        string Dump(int index, int size);
        string Dump();
        IFormalValue RefPlus(int right);
        void Check(IEnumerable<FormalValueAccess> accesses);
    }
}