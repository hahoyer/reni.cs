using System;
using Reni.Runtime;

namespace reni.Generated
{
    /// <summary>
    /// sss
    /// </summary>
    public unsafe class reni_Test
    {
        // 
        public static void reni()
        {
            fixed (sbyte* data = new sbyte[2])
            {
                (*(Int16*)(data + 0)) = (Int16)(128); // BitArray 153
                reni_0((data + 2)); // Call     420


            };
        }

        // Reni.Syntax.Struct
        // {
        //     _data=Reni.Struct
        //     {
        //         C:\data\develop\out\Debug\struct.1(2,0): see there: 
        //         
        //     }
        //     ObjectId=61
        // }
        private static void reni_0(System.SByte* frame)
        {
            fixed (sbyte* data = new sbyte[2])
            {
                (*(sbyte*)(data + 1)) = (sbyte)(127); // BitArray                       840
                (*(Int16*)(data + 0)) = (*(Int16*)(frame - 2)); // TopFrame                       1119
                (*(sbyte*)(data + 1)) = (*(sbyte*)(data + 0)); // StatementEnd                    926
                (*(sbyte*)(data + 0)) = (*(sbyte*)(data + 1)); // TopData                         1060
                Data.DumpPrint((*(sbyte*)(data + 0))); // DumpPrint                        993
                (*(sbyte*)(frame - 1)) = (*(sbyte*)(data + 1)); // StorageDescriptor.FunctionReturn

            };
        }
    }
}