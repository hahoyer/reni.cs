using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Runtime;

namespace Reni
{
    /// <summary>
    /// sss
    /// </summary>
    public unsafe class reni_Test
    {
        /// <summary>
        /// sss
        /// </summary>
        // 
        public static void reni()
        {
            fixed(sbyte* data = new sbyte[10])
            {
                var constant_1 = (data + 9);
                var constant_4 = (data + 8);
                var constant_2050 = (Int16*)(data + 6);

                (*constant_1) = (1); // BitArray 4253
                (*constant_4) = (4); // BitArray 4254
                (*constant_2050) = (2050); // BitArray 4255
                
                (*(data + 5)) = (*(data + 11)); // TopData 4257
                (*(data + 5)) = (sbyte) ((sbyte) ((*(data + 5)) << 6) >> 6); // BitCast 539
                (*(data + 4)) = (*(data + 10)); // TopData 4259
                (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 4) >> 4); // BitCast 541
                (*(data + 5)) = (sbyte) ((*(data + 5)) + (*(data + 4))); // BitArrayBinaryOp 4058
                (*(data + 5)) = (sbyte) ((sbyte) ((*(data + 5)) << 3) >> 3); // BitCast 543
                (*(data + 4)) = (*(data + 5)); // TopData 4261
                (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 3) >> 3); // BitCast 545
                (*(Int16*) (data + 2)) = (*(Int16*) constant_4); // TopData 4263
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 3) >> 3); // BitCast 547
                (*(Int16*) (data + 3)) = (Int16) ((*(data + 4)) + (*(Int16*) (data + 2))); // BitArrayBinaryOp 4066
                (*(Int16*) (data + 3)) = (Int16) ((Int16) ((*(Int16*) (data + 3)) << 2) >> 2); // BitCast 549
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 3)); // StatementEnd 4073
                (*(Int16*) (data + 2)) = (*(Int16*) (data + 4)); // TopData 4265
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 2) >> 2); // BitCast 551
                Data.DumpPrint((*(Int16*) (data + 2))); // DumpPrint 4089
            }
            ;
        }
    }
}