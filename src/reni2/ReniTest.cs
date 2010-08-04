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
            fixed(sbyte* data = new sbyte[17])
            {
                (*(data + 16)) = (1); // BitArray         1479
                (*(data + 15)) = (11); // BitArray         1480
                (*(data + 14)) = (3); // BitArray         1481
                (*(Int32*) (data + 10)) = (Int32) (data + 14); // TopRef           1482
                (*(data + 9)) = (4); // BitArray         1483
                (*(data + 8)) = (*(data + 9)); // TopData          1485
                (*(data + 8)) = (sbyte) ((sbyte) ((*(data + 8)) << 4) >> 4); // BitCast          285
                (*(Int32*) (data + 4)) = (Int32) (data + 10); // TopRef           1486
                (*(Int32*) (data + 0)) = (Int32) (data + 8); // TopRef           1487
                *((sbyte*) (*(Int32*) (data + 4))) = *((sbyte*) (*(Int32*) (data + 0))); // Assign           1425
                Data.DumpPrint("("); // DumpPrintText    1398
                (*(data + 13)) = (*(data + 16)); // TopData          1489
                (*(data + 13)) = (sbyte) ((sbyte) ((*(data + 13)) << 6) >> 6); // BitCast          273
                Data.DumpPrint((*(data + 13))); // DumpPrintOperation 1389
                Data.DumpPrint(",                 "); // DumpPrintText      1399
                (*(data + 13)) = (*(data + 15)); // TopData          1491
                (*(data + 13)) = (sbyte) ((sbyte) ((*(data + 13)) << 3) >> 3); // BitCast          275
                Data.DumpPrint((*(data + 13))); // DumpPrintOperation 1391
                Data.DumpPrint(",                 "); // DumpPrintText      1400
                (*(data + 13)) = (*(data + 14)); // TopData          1493
                (*(data + 13)) = (sbyte) ((sbyte) ((*(data + 13)) << 5) >> 5); // BitCast          277
                Data.DumpPrint((*(data + 13))); // DumpPrintOperation 1393
                Data.DumpPrint(",                 "); // DumpPrintText      1401
                Data.DumpPrint("                )"); // DumpPrintText      1402
            }
        }
    }
}