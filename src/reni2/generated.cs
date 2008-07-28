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
            fixed(sbyte* data = new sbyte[12])
            {
                (*(data + 11)) = (1); // BitArray      4046
                (*(data + 10)) = (11); // BitArray      4047
                (*(data + 9)) = (3); // BitArray      4048
                (*(data + 8)) = (3); // BitArray      4049
                (*(Int32*) (data + 4)) = (Int32) (data + 10); // TopRef        4050
                (*(Int32*) (data + 0)) = (Int32) (data + 8); // TopRef        1916
                *((sbyte*) (*(Int32*) (data + 4))) = *((sbyte*)(*(Int32*) (data + 0))); // Assign        2495
                Data.MoveBytes(0, (data + 9), (data + 8)); // StatementEnd  2728
                Data.DumpPrint("("); // DumpPrintText 3812
                (*(data + 8)) = (*(data + 11)); // TopData     4053
                (*(data + 8)) = (sbyte) ((sbyte) ((*(data + 8)) << 6) >> 6); // BitCast   4054
                Data.DumpPrint((*(data + 8))); // DumpPrint   3154
                Data.DumpPrint(",                "); // DumpPrintText 3820
                (*(data + 8)) = (*(data + 10)); // TopData     4057
                (*(data + 8)) = (sbyte) ((sbyte) ((*(data + 8)) << 3) >> 3); // BitCast   4058
                Data.DumpPrint((*(data + 8))); // DumpPrint   3273
                Data.DumpPrint(",                "); // DumpPrintText 3835
                (*(data + 8)) = (*(data + 9)); // TopData     4061
                (*(data + 8)) = (sbyte) ((sbyte) ((*(data + 8)) << 5) >> 5); // BitCast   4062
                Data.DumpPrint((*(data + 8))); // DumpPrint   3395
                Data.DumpPrint(",                "); // DumpPrintText 3856
                Data.DumpPrint("               )"); // DumpPrintText 3880
                Data.MoveBytes(0, (data + 12), (data + 9)); // StatementEnd  4025
            }
            ;
        }
    }
}