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
                (*(sbyte*)(data + 9)) = (sbyte)(100); // BitArray         491
                (*(sbyte*)(data + 8)) = (sbyte)(2); // BitArray         561
                (*(Int32*)(data + 4)) = (Int32)(data + 10); // TopRef           562
                (*(Int32*)(data + 0)) = (Int32)(data + 12); // TopRef           563
                Function0((data + 8)); // Call             510
                (*(Int16*)(data + 6)) = (Int16)((Int16)((*(Int16*)(data + 6)) << 7) >> 7); // BitCast          84
                Data.DumpPrint((*(Int16*)(data + 6))); // DumpPrintOperation 503


            };
        }

        // (arg)+(x)
        private static void Function0(System.SByte* frame)
        {
            fixed (sbyte* data = new sbyte[5])
            {
            StartFunction:
                (*(Int32*)(data + 1)) = (*(Int32*)(frame - 8)); // TopFrame         565
                (*(sbyte*)(data + 4)) = (*(sbyte*)(*(Int32*)(data + 1))); // Dereference      547
                (*(sbyte*)(data + 4)) = (sbyte)((sbyte)((*(sbyte*)(data + 4)) << 5) >> 5); // BitCast          90
                (*(Int32*)(data + 0)) = (*(Int32*)(frame - 4)); // TopFrame         567
                (*(Int32*)(data + 0)) += -1; // RefPlus          550
                (*(sbyte*)(data + 3)) = (*(sbyte*)(*(Int32*)(data + 0))); // Dereference      549
                (*(Int16*)(data + 3)) = (Int16)((*(sbyte*)(data + 4)) + (*(sbyte*)(data + 3))); // BitArrayBinaryOp 551
                (*(Int16*)(data + 3)) = (Int16)((Int16)((*(Int16*)(data + 3)) << 7) >> 7); // BitCast          92
                (*(Int16*)(frame - 2)) = (*(Int16*)(data + 3)); // FunctionReturn  

            };
        }
    }
}
