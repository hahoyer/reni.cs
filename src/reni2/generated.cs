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
            fixed(sbyte* data = new sbyte[3])
            {
                (*(data + 2)) = (4); // BitArray 136
                (*(data + 1)) = (*(data + 2)); // TopData 401
                reni_0((data + 2)); // Call    274
                (*(data + 0)) = (*(data + 1)); // TopData 330
                Data.DumpPrint((*(data + 0))); // DumpPrint 316
            }
            ;
        }

        // f(arg)
        private static void reni_0(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (Int32) (frame - 1); // FrameRef                         504
                reni_1((data + 4)); // Call                             481
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.FunctionReturn
            }
            ;
        }

        // arg
        private static void reni_1(SByte* frame)
        {
            fixed(sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (Int32) (frame - 4); // FrameRef                       540
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference                    530
                (*(frame - 1)) = (*(data + 3)); // StorageDescriptor.FunctionReturn
            }
            ;
        }
    }
}