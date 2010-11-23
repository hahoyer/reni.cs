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
        public static void MainFunction() {
            var data = new DataContainer();
            var local1Index0 = new DataContainer(2);
            data.Expand((local1Index0.DataRef(0)).Call(Function0).BitCast(1, 4).DumpPrint());
            data.DropAll();
            ;
        }
        
        // (arg)+(1)
        private static DataContainer Function0(DataContainer frame) {
            StartFunction:
            var data = new DataContainer();
            var local4Index0 = (frame.DataPartFromBack(-4, 4)).Dereference(1).BitCast(1, 5);
            var local4Index1 = new DataContainer(1);
            data.Expand((local4Index0.DataPart(0, 1)).BitCast(1, 5));
            data.Expand((local4Index1.DataPart(0, 1)).BitCast(1, 6));
            data.Expand((data).Plus(1, 1).BitCast(1, 4));
            return data;
            ;
        }
    }
}