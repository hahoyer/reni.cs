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
            fixed(sbyte* data = new sbyte[6])
            {
                (*(Int16*) (data + 4)) = (1000); // BitArray         129
                (*(Int32*) (data + 0)) = (Int32) (data + 6); // TopRef           130
                Function0((data + 4)); // Call             20
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 4) >> 4); // BitCast          3
                Data.DumpPrint((*(Int16*) (data + 2))); // DumpPrintOperation 15
            }
            ;
        }

        // (container.0)_A_T_(2)
        private static void Function0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[9])
            {
                (*(data + 8)) = (3); // BitArray   131
                (*(Int32*) (data + 4)) = (Int32) (data + 9); // TopRef     132
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame   134
                Function1((data + 8)); // Call         60
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 6)); // TopData       136
                (*(Int16*) (data + 7)) = (*(Int16*) (data + 4)); // LocalBlockEnd  80
                (*(Int16*) (frame - 2)) = (*(Int16*) (data + 7)); // FunctionReturn
            }
            ;
        }

        // (x)+(y)
        private static void Function1(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[8])
            {
                (*(Int32*) (data + 4)) = (*(Int32*) (frame - 4)); // TopFrame      138
                (*(Int32*) (data + 4)) += -2; // RefPlus       94
                (*(Int16*) (data + 6)) = (*(Int16*) (*(Int32*) (data + 4))); // Dereference   112
                (*(Int16*) (data + 6)) = (Int16) ((Int16) ((*(Int16*) (data + 6)) << 5) >> 5); // BitCast       16
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 6)); // TopData         140
                (*(Int16*) (data + 4)) = (Int16) ((Int16) ((*(Int16*) (data + 4)) << 5) >> 5); // BitCast         6
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 8)); // TopFrame         142
                (*(Int32*) (data + 0)) += -1; // RefPlus          104
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference      101
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast          8
                (*(Int16*) (data + 4)) = (Int16) ((*(Int16*) (data + 4)) + (*(data + 3))); // BitArrayBinaryOp 106
                (*(Int16*) (data + 4)) = (Int16) ((Int16) ((*(Int16*) (data + 4)) << 4) >> 4); // BitCast          10
                (*(Int16*) (data + 6)) = (*(Int16*) (data + 4)); // LocalBlockEnd    117
                (*(Int16*) (frame - 2)) = (*(Int16*) (data + 6)); // FunctionReturn  
            }
            ;
        }
    }
}