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
            DataContainer top = null;
            var list1 = top;
            top = new DataContainer();
            top.Expand(new DataContainer(5));
            top.Expand(new DataContainer(6));
            var local2Index0 = top;
            (local2Index0.DataPart(1, 1)).BitCast(1, 4).DumpPrint().DropAll();
            top = list1;
            list1.DropAll();
            ;
        }
    }
}