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
                (*(data + 5)) = (31); // BitArray 13210
                (*(data + 4)) = (100); // BitArray 13254
                (*(Int32*) (data + 0)) = (Int32) (data + 4); // TopRef 13224
                (*(Int32*) (data + 1)) = (*(Int32*) (data + 0)); // StatementEnd 13306
                (*(data + 4)) = (*(sbyte*) (*(Int32*) (data + 1))); // Dereference 13357
                (*(data + 3)) = (*(data + 4)); // TopData 13421
                Data.DumpPrint((*(data + 3))); // DumpPrint 13407
            }
            ;
        }
    }
}