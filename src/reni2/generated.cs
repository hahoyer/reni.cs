using System;
using Reni.Runtime;

namespace reni.Generated
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
            fixed (sbyte* data = new sbyte[5])
            {
                (*(Int16*)(data + 3)) = (Int16)(128); // BitArray 241
                reni_0((data + 5)); // Call 512
                (*(Int32*)(data + 0)) = (Int32)(data + 4); // TopRef 576
                reni_1((data + 4)); // Call 601

            };
        }

        // Reni.Syntax.Struct
        // {                                           
        //     _data=Reni.Struct.Container
        //     {
        //         C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.4(2,0): see there: 
        //         
        //     }
        //     ObjectId=150
        // }
        private static void reni_0(System.SByte* frame)
        {
            fixed (sbyte* data = new sbyte[5])
            {
            StartFunction:
                (*(sbyte*)(data + 4)) = (sbyte)(127); // BitArray 765
                (*(Int16*)(data + 3)) = (*(Int16*)(frame - 2)); // TopFrame 1029
                (*(sbyte*)(data + 4)) = (*(sbyte*)(data + 3)); // StatementEnd 879
                (*(Int32*)(data + 0)) = (Int32)(data + 5); // TopRef 935
                reni_1((data + 4)); // Call 906
                (*(sbyte*)(frame - 1)) = (*(sbyte*)(data + 4)); // StorageDescriptor.FunctionReturn
            };
        }

        // Reni.Syntax.Statement
        // {
        //     Chain=Count=2
        //     {
        //         [0] Reni.Syntax.MemberElem
        //         {
        //             DefineableToken=Reni.Parser.DefineableToken
        //             {
        //                 TokenClass=Reni.Parser.TokenClass.UserSymbol
        //                 {
        //                     PotentialTypeName=.TokenClass.Name.T_dataT
        //                     DataFunctionName=UserSymbol
        //                     ObjectId=29
        //                 }
        //                 ObjectId=87
        //             }
        //             Args=null
        //             ObjectId=5
        //         }
        //         [1] Reni.Syntax.MemberElem
        //         {
        //             DefineableToken=Reni.Parser.DefineableToken
        //             {
        //                 TokenClass=Reni.Parser.TokenClass.Name.Tdump_printT
        //                 {
        //                     DataFunctionName=Tdump_printT
        //                     ObjectId=30
        //                 }
        //                 ObjectId=86
        //             }
        //             Args=null
        //             ObjectId=6
        //         }
        //     }
        //     ObjectId=88
        // }
        private static void reni_1(System.SByte* frame)
        {
            fixed (sbyte* data = new sbyte[4])
            {
            StartFunction:
                (*(Int32*)(data + 0)) = (*(Int32*)(frame - 4)); // TopFrame 1159
                (*(Int32*)(data + 0)) += -1; // RefPlus 1093
                (*(sbyte*)(data + 3)) = (*(sbyte*)(*(Int32*)(data + 0))); // Dereference 1104
                Data.DumpPrint((*(sbyte*)(data + 3))); // DumpPrint 1116
                ; // StorageDescriptor.FunctionReturn
            };
        }
    }
}