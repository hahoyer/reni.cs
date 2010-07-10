using System;
using System.Collections.Generic;
using System.Linq;
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
            fixed (sbyte* data = new sbyte[10])
            {
                var p5 = data + 9;
                var p6 = (sbyte*)(data + 8);
                *p5 = (sbyte)(5); // BitArray         44
                *p6 = (sbyte)(6); // BitArray         45
                var data4 = (Int32*)(data + 4);
                (*data4) = (Int32)(data + 8); // TopRef           46
                (*(sbyte*)(data + 3)) = (*(sbyte*)(data + 4)); // TopData          48
                (*(sbyte*)(data + 3)) = (sbyte)((sbyte)((*(sbyte*)(data + 3)) << 4) >> 4); // BitCast          4
                Data.DumpPrint((*(sbyte*)(data + 3))); // DumpPrintOperation 18


            };
        }
    }
}
