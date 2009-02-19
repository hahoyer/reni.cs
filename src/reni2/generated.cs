using System;
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
            fixed (sbyte* data = new sbyte[9])
            {
                (*(data + 8)) = (4); // BitArray 6723
                (*(Int32*) (data + 4)) = (Int32) (data + 9); // TopRef  6724
                (*(data + 3)) = (*(data + 12)); // TopData 6726
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 4) >> 4); // BitCast 5698
                reni_0((data + 8)); // Call    5822
                (*(data + 6)) = (*(data + 7)); // TopData 6728
                (*(data + 6)) = (sbyte) ((sbyte) ((*(data + 6)) << 4) >> 4); // BitCast 5893
                Data.DumpPrint((*(data + 6))); // DumpPrint 5886
            }
            ;
        }

        // f(arg)
        private static void reni_0(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(data + 3)) = (*(frame - 5)); // TopFrame                           6743
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 4) >> 4); // BitCast                            6522
                reni_1((data + 4)); // Call                                 6561
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.CreateFunctionReturn
            }
            ;
        }

        // arg
        private static void reni_1(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(data + 3)) = (*(frame - 1)); // TopFrame                             6764
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 4) >> 4);
                    // BitCast                              6650
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.CreateFunctionReturn
            }
            ;
        }
    }
}