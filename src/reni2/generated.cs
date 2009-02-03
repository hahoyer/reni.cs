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
            fixed (sbyte* data = new sbyte[7])
            {
                (*(sbyte*)(data + 6)) = (sbyte)(10); // BitArray     22534
                var a = Data.ToInt64(1, (data + 6));
                Data.MoveBytes(3, (data + 3), (data + 6)); // TopData      22536
                var b = Data.ToInt64(3, (data + 3));
                Data.BitCast(3, (data + 3), 19); // BitCast      18326
                var c = Data.ToInt64(3, (data + 3));
                Data.MoveBytes(3, (data + 4), (data + 3)); // StatementEnd 18361
                (*(Int32*)(data + 0)) = (Int32)(data + 7); // TopRef     22537
                reni_0((data + 4)); // Call       20617


            };
        }

        // ((i)>(0))then(container.22)
        private static void reni_0(System.SByte* frame)
        {
            fixed (sbyte* data = new sbyte[15])
            {
            StartFunction:
                (*(sbyte*)(data + 14)) = (sbyte)(0); // BitArray     22549
                (*(Int32*)(data + 10)) = (*(Int32*)(frame - 4)); // TopFrame     22551
                (*(Int32*)(data + 10)) += -3; // RefPlus      21179
                Data.MoveBytes(3, (data + 11), ((sbyte*)(*(Int32*)(data + 10)))); // Dereference  21204
                Data.BitCast(3, (data + 11), 4); // BitCast      21205
                (*(sbyte*)(data + 10)) = (*(sbyte*)(data + 14)); // TopData      22553
                (*(sbyte*)(data + 10)) = (sbyte)((sbyte)((*(sbyte*)(data + 10)) << 7) >> 7); // BitCast      21133
                var a = Data.ToInt64(3, (data + 11));
                var b = Data.ToInt64(1, (data + 10));
                Data.Greater(1, (data + 13), 3, (data + 11), 1, (data + 10)); // BitArrayOp   21244
                var c = Data.ToInt64(1, (data + 13));

                (*(sbyte*)(data + 13)) = (sbyte)((sbyte)((*(sbyte*)(data + 13)) << 7) >> 7); // BitCast      21245
                (*(sbyte*)(data + 14)) = (*(sbyte*)(data + 13)); // StatementEnd 21274
                if ((*(sbyte*)(data + 14)) != 0)
                {
                    ; // Then        22555
                    (*(sbyte*)(data + 14)) = (sbyte)(1); // BitArray    22556
                    (*(Int32*)(data + 10)) = (*(Int32*)(frame - 4)); // TopFrame    22558
                    (*(Int32*)(data + 10)) += -3; // RefPlus     21507
                    Data.MoveBytes(3, (data + 11), ((sbyte*)(*(Int32*)(data + 10)))); // Dereference 21532
                    Data.BitCast(3, (data + 11), 4); // BitCast   21533
                    (*(sbyte*)(data + 10)) = (*(sbyte*)(data + 14)); // TopData  22560
                    (*(sbyte*)(data + 10)) = (sbyte)((sbyte)((*(sbyte*)(data + 10)) << 6) >> 6); // BitCast  21455
                    Data.Minus(3, (data + 11), 3, (data + 11), 1, (data + 10)); // BitArrayOp 21547
                    Data.BitCast(3, (data + 11), 3); // BitCast    21548
                    Data.MoveBytes(3, (data + 8), (data + 11)); // TopData    22562
                    Data.BitCast(3, (data + 8), 4); // BitCast    21623
                    (*(Int32*)(data + 4)) = (*(Int32*)(frame - 4)); // TopFrame   22564
                    (*(Int32*)(data + 4)) += -3; // RefPlus    21679
                    (*(Int32*)(data + 0)) = (Int32)(data + 8); // TopRef     22565
                    Data.MoveBytes(3, (sbyte*)(*(Int32*)(data + 4)), (sbyte*)(*(Int32*)(data + 0))); // Assign     21350
                    (*(Int32*)(data + 11)) = (*(Int32*)(frame - 4)); // TopFrame   22567
                    (*(Int32*)(data + 11)) += -3; // RefPlus    21896
                    Data.MoveBytes(3, (data + 12), ((sbyte*)(*(Int32*)(data + 11)))); // Dereference 21926
                    Data.BitCast(3, (data + 12), 4); // BitCast     21927
                    Data.DumpPrint(3, (data + 12)); // DumpPrint   21920
                    goto StartFunction; // RecursiveCall 22570
                }
                else
                {
                    ; // Else          22571
                };                                    //                                                      EndCondional 22572
                ;                                    //                                                      StorageDescriptor.CreateFunctionReturn

            };
        }
    }
}
