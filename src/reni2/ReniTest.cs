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
        public static void Reni()
        {
            fixed(sbyte* data = new sbyte[8])
            {
                var varX = data + 7;
                var constant2 = data + 6;
                (*varX) = (100); // BitArray 4325
                (*constant2) = (2); // BitArray 4443
                var constant2Ref = (Int32*) (data + 2);
                (*constant2Ref) = (Int32) constant2; // TopRef 4444
                Reni0(constant2); // Call 4344
                (*(Int16*) (data + 2)) = (*(Int16*) (data + 4)); // TopData 4446
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 7) >> 7); // BitCast 622
                Data.DumpPrint((*(Int16*) (data + 2))); // DumpPrint 4337
            }
            ;
        }

        // (arg)+(x)
        private static void Reni0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[5])
            {
                var argRefRef = (Int32*) (frame - 4);
                var dataArgRefRef = (Int32*) (data + 1);
                (*dataArgRefRef) = (*argRefRef); // TopFrame 4448
                var argRef = (sbyte*) (*dataArgRefRef);
                var arg = (data + 4);
                (*arg) = (*argRef); // Dereference 4429
                (*arg) = (sbyte) ((sbyte) ((*arg) << 5) >> 5); // BitCast 625
                (*(data + 3)) = (*(data + 5)); // TopData 4450
                (*(Int16*) (data + 3)) = (Int16) ((*arg) + (*(data + 3))); // BitArrayBinaryOp 4431
                (*(Int16*) (data + 3)) = (Int16) ((Int16) ((*(Int16*) (data + 3)) << 7) >> 7); // BitCast 627
                (*(Int16*) (frame - 2)) = (*(Int16*) (data + 3)); // FunctionReturn
            }
            ;
        }
    }
}