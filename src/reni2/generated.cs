using System;
using HWClassLibrary.Debug;
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
        public static void reni()
        {
            fixed(sbyte* data = new sbyte[5])
            {
                reni_0((data + 5)); // Call    1189
                (*(data + 3)) = (*(data + 4)); // TopData 2449
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast 1385
                Data.DumpPrint((*(data + 3))); // DumpPrint 1378
            }
            ;
        }

        // (container.0)_A_T_(2)
        private static void reni_0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[6])
            {
                (*(data + 5)) = (3); // BitArray                           2467
                (*(Int32*) (data + 1)) = (Int32) (data + 4); // TopRef                             2468
                (*(data + 0)) = (2); // BitArray                           2469
                reni_1((data + 5)); // Call                               1978
                (*(data + 3)) = (*(data + 4)); // TopData                            2471
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast                            2196
                (*(data + 5)) = (*(data + 3)); // StatementEnd                         2225
                (*(frame - 1)) = (*(data + 5)); // StorageDescriptor.CreateFunctionReturn
            }
            ;
        }

        // y
        private static void reni_1(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame                             2490
                (*(Int32*) (data + 0)) += -1; // RefPlus                              2340
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference                          2359
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5);
                    // BitCast                              2360
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.CreateFunctionReturn
            }
            ;
        }
    }
}