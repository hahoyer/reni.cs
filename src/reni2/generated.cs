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
            fixed(sbyte* data = new sbyte[7])
            {
                (*(data + 6)) = (3); // BitArray 3427
                (*(Int32*) (data + 2)) = (Int32) (data + 7); // TopRef   3428
                (*(data + 1)) = (2); // BitArray 3429
                reni_1((data + 6)); // Call     1115
                reni_0((data + 5)); // Call     1804
                (*(data + 3)) = (*(data + 4)); // TopData  3430
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast  2013
                Data.DumpPrint((*(data + 3))); // DumpPrint  1996
                Data.MoveBytes(0, (data + 7), (data + 4)); // StatementEnd 2234
            }
        }

        // (container.0)_A_T_(2)
        private static void reni_0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[6])
            {
                (*(data + 5)) = (3); // BitArray                       3439
                (*(Int32*) (data + 1)) = (Int32) (data + 6); // TopRef                         3440
                (*(data + 0)) = (2); // BitArray                       3441
                reni_1((data + 5)); // Call                           1115
                (*(Int32*) (data + 0)) = (Int32) (data + 4); // TopRef                         3442
                (*(Int32*) (data + 2)) = (*(Int32*) (data + 0)); // StatementEnd                   3024
                (*(data + 5)) = (*(sbyte*) (*(Int32*) (data + 2))); // Dereference                    3060
                (*(data + 5)) = (sbyte) ((sbyte) ((*(data + 5)) << 5) >> 5); // BitCast                        3061
                (*(frame - 1)) = (*(data + 5)); // StorageDescriptor.FunctionReturn
            }
        }

        // y
        private static void reni_1(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame                       3452
                (*(Int32*) (data + 0)) += -1; // RefPlus                        3132
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference                    3183
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast                        3184
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.FunctionReturn
            }
        }
    }
}