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
        /// <summary>
        /// sss
        /// </summary>
        // 
        public static void MainFunction()
        {
            fixed(sbyte* data = new sbyte[10])
            {
                (*(data + 9)) = (100); // BitArray 4280
                (*(data + 8)) = (2); // BitArray 4412
                (*(Int32*) (data + 4)) = (Int32) (data + 10); // TopRef 4413
                (*(Int32*) (data + 0)) = (Int32) (data + 12); // TopRef 4414
                ReniFunction0((data + 8)); // Call 4300
                (*(Int16*) (data + 4)) = (*(Int16*) (data + 6)); // TopData 4416
                (*(Int16*) (data + 4)) = (Int16) ((Int16) ((*(Int16*) (data + 4)) << 7) >> 7); // BitCast 514
                Data.DumpPrint((*(Int16*) (data + 4))); // DumpPrintOperation 4292
            }
        }

        // (arg)+(x)
        private static void ReniFunction0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[5])
            {
                var argRefRef = (Int32*) (data + 1);
                (*argRefRef) = (*(Int32*) (frame - 8)); // TopFrame 4418
                var argRef = (data + 4);
                (*argRef) = (*(sbyte*) (*argRefRef)); // Dereference 4400
                (*argRef) = (sbyte) ((sbyte) ((*argRef) << 5) >> 5); // BitCast 517
                var contextRef = (Int32*) (data + 0);
                (*contextRef) = (*(Int32*) (frame - 4)); // TopFrame 4420
                (*contextRef) += -1; // RefPlus 4398
                var xRef = (data + 3);
                (*xRef) = (*(sbyte*) (*contextRef)); // Dereference 4401
                var result = (Int16*) xRef;
                (*result) = (Int16) ((*argRef) + (*xRef)); // BitArrayBinaryOp 4402
                (*result) = (Int16) ((Int16) ((*result) << 7) >> 7); // BitCast 519
                (*(Int16*) (frame - 2)) = (*result); // FunctionReturn
            }
        }
    }
}