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
            fixed (sbyte* data = new sbyte[10])
            {
                (*(data + 9)) = (1); // BitArray 33973
                reni_0((data + 10)); // Call   17259
                (*(Int32*) (data + 5)) = (Int32) (data + 10); // TopRef 33974
                (*(Int32*) (data + 1)) = (Int32) (data + 10); // TopRef 33975
                (*(data + 0)) = (2); // BitArray 33976
                reni_1((data + 1)); // Call     11097
                reni_2((data + 9)); // Call    16219
                (*(Int32*) (data + 4)) = (Int32) (data + 9); // TopRef 33977
                reni_8((data + 8)); // Call   10066
            }
            ;
        }

        // container.0
        private static void reni_0(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[5])
            {
                (*(data + 4)) = (*(frame - 1)); // TopFrame 34262
                (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 6) >> 6); // BitCast  18277
                (*(data + 3)) = (*(data + 4)); // TopData    34264
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 6) >> 6); // BitCast    18318
                (*(data + 4)) = (*(data + 3)); // StatementEnd 18353
                (*(frame - 1)) = (*(data + 4)); // FunctionReturn
            }
            ;                                                                   
        }

        // container.0
        private static void reni_1(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[5])
            {
                (*(data + 4)) = (*(frame - 1)); // TopFrame 34549
                (*(data + 4)) = (sbyte) ((sbyte) ((*(data + 4)) << 5) >> 5); // BitCast  19317
                (*(data + 3)) = (*(data + 4)); // TopData    34551
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << 5) >> 5); // BitCast    19358
                (*(data + 4)) = (*(data + 3)); // StatementEnd 19393
                (*(frame - 1)) = (*(data + 4)); // FunctionReturn
            }
            ;
        }

        // create((_data)+((create(arg))_data))
        private static void reni_2(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[10])
            {
                (*(Int32*) (data + 6)) = (*(Int32*) (frame - 4)); // TopFrame    34866
                (*(data + 5)) = (*(frame - 9)); // TopFrame    34868
                reni_3((data + 10)); // Call        22256
                (*(Int32*) (data + 5)) = (*(Int32*) (frame - 4)); // TopFrame    34870
                (*(Int32*) (data + 1)) = (*(Int32*) (frame - 8)); // TopFrame    34872
                (*(Int32*) (data + 1)) += -1; // RefPlus     22956
                (*(data + 4)) = (*(sbyte*) (*(Int32*) (data + 1))); // Dereference 23006
                (*(data + 3)) = (*(data + 9)); // TopData    34874
                (*(Int16*) (data + 3)) = (Int16) ((*(data + 4)) + (*(data + 3))); // BitArrayOp 23057
                (*(Int16*) (data + 3)) = (Int16) ((Int16) ((*(Int16*) (data + 3)) << 7) >> 7); // BitCast    23058
                reni_6((data + 9)); // Call       23780
                (*(data + 9)) = (*(data + 8)); // StatementEnd 24309
                (*(frame - 1)) = (*(data + 9)); // FunctionReturn
            }
            ;
        }

        // Integer8(arg)
        private static void reni_3(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(data + 3)) = (*(frame - 5)); // TopFrame    35189
                reni_4((data + 4)); // Call          25775
                (*(frame - 1)) = (*(data + 3)); // FunctionReturn
            }
            ;
        }

        // container.0
        private static void reni_4(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[5])
            {
                (*(Int32*) (data + 1)) = (Int32) (frame - 0); // FrameRef      35503
                reni_5((data + 5)); // Call          26761
                (*(data + 3)) = (*(data + 4)); // TopData       35505
                (*(data + 4)) = (*(data + 3)); // StatementEnd   27195
                (*(frame - 1)) = (*(data + 4)); // FunctionReturn
            }
            ;
        }

        // (_data)enable_cut
        private static void reni_5(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame     35518
                (*(Int32*) (data + 0)) += -1; // RefPlus      28260
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference  28283
                (*(frame - 1)) = (*(data + 3)); // FunctionReturn
            }
            ;
        }

        // Integer8(arg)
        private static void reni_6(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(Int16*) (data + 2)) = (*(Int16*) (frame - 6)); // TopFrame   35803
                (*(Int16*) (data + 2)) = (Int16) ((Int16) ((*(Int16*) (data + 2)) << 7) >> 7); // BitCast    28391
                reni_7((data + 4)); // Call         28958
                (*(frame - 1)) = (*(data + 3)); // FunctionReturn
            }
            ;
        }

        // container.0
        private static void reni_7(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[6])
            {
                (*(Int16*) (data + 4)) = (*(Int16*) (frame - 2)); // TopFrame   36088
                (*(Int16*) (data + 4)) = (Int16) ((Int16) ((*(Int16*) (data + 4)) << 7) >> 7); // BitCast    29707
                (*(data + 3)) = (*(data + 4)); // TopData    36090
                (*(data + 3)) = (sbyte) ((sbyte) ((*(data + 3)) << -1) >> -1); // BitCast    29748
                (*(data + 5)) = (*(data + 3)); // StatementEnd 29784
                (*(frame - 1)) = (*(data + 5)); // FunctionReturn
            }
            ;
        }

        // (_data)dump_print
        private static void reni_8(SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
                (*(Int32*) (data + 0)) = (*(Int32*) (frame - 4)); // TopFrame    36103
                (*(Int32*) (data + 0)) += -1; // RefPlus     30711
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference 30731
                Data.DumpPrint((*(data + 3))); // DumpPrint   30741
                ; //                                                 FunctionReturn
            }
            ;
        }
    }
}