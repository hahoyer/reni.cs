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
            fixed(sbyte* data = new sbyte[6])
            {
                (*(data + 5)) = (2); // BitArray   1351
                (*(data + 4)) = (*(data + 5)); // TopData    1434
                (*(data + 3)) = (4); // BitArray   1424
                (*(data + 4)) = (sbyte) ((*(data + 4)) + (*(data + 3))); // BitArrayOp 1427
                (*(Int32*) (data + 0)) = (*(Int32*) (data + 4)); // TopData    1525
                (*(data + 3)) = (*(sbyte*) (*(Int32*) (data + 0))); // Dereference 1510
                Data.DumpPrint((*(data + 3))); // DumpPrint   1498
            }
            ;
        }
    }
}