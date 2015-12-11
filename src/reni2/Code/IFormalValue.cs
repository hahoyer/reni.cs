using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.Code
{
    internal interface IFormalValue
    {
        string Dump(int index, int size);
        string Dump();
        IFormalValue RefPlus(int right);
        void Check(FormalValueAccess[] accesses);
    }
}