using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Runtime;

namespace Reni
{
    /// <summary>
    /// sss
    /// </summary>
    public static unsafe class ReniTest
    {
        public static void MainFunction()
        {
            var list0 = new DataContainer();
            var local0Index0 = new DataContainer(2);
            var local0Index1 = new DataContainer(4);
            var list1 = new DataContainer();
            list1.Expand((local0Index0.DataPart(1)).BitCast(1, 3));
            list1.Expand((local0Index1.DataPart(1)).BitCast(1, 4));
            list0.Expand((list1).Plus(1, 1).BitCast(1, 5).DumpPrint(8));
            list0.Drop();
            ;
        }
    }
}