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
            fixed(sbyte* data = new sbyte[5])
            {
                (*(data + 4)) = (100); // BitArray         2
                (*(Int32*) (data + 0)) = (Int32) (data + 5); // TopRef           124
                Function0((data + 4)); // Call             20
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 7) >> 7); // BitCast          2
                Data.DumpPrint((*(Int16*) (data + 2))); // DumpPrintOperation 15
            }
            ;
        }

        // (container.0)_A_T_(2)
        private static void Function0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[9])
            {
                (*(data + 8)) = (3); // BitArray   125
                (*(Int32*) (data + 4)) = (Int32) (data + 9); // TopRef     126
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame   128
                Function1((data + 8)); // Call         60
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 6)); // TopData       130
                (*(Int16*) (data + 7)) = (*(Int16*) (data + 4)); // LocalBlockEnd  80
                (*(Int16*) (frame - 2)) = (*(Int16*) (data + 7)); // FunctionReturn
            }
            ;
        }

        // (x)+(y)
        private static void Function1(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[6])
            {
                (*(Int32*) (data + 2)) = (*(Int32*) (frame - 4)); // TopFrame         132
                (*(Int32*) (data + 2)) += -1; // RefPlus          94
                (*(data + 5)) = (*(sbyte*) (*(Int32*) (data + 2))); // Dereference      90
                (*(data + 4)) = (*(data + 5)); // TopData          134
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 8)); // TopFrame         136
                (*(Int32*) (data + 0)) += -1; // RefPlus          103
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference      100
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast          5
                (*(Int16*) (data + 3)) = (Int16) ((*(data + 4)) + (*(data + 3))); // BitArrayBinaryOp 105
                (*(Int16*) (data + 3)) = (Int16) ((Int16) ((*(Int16*) (data + 3)) << 7) >> 7); // BitCast          7
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 3)); // LocalBlockEnd    112
                (*(Int16*) (frame - 2)) = (*(Int16*) (data + 4)); // FunctionReturn  
            }
            ;
        }
    }
}