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
            fixed(sbyte*data=new sbyte[5])
            {
                (*(sbyte*) (data+4)) = (sbyte)(2)   ; // BitArray 240
                reni_0((   data+5))                  ; // Call   410
                (*(Int32*) (data+0)) = (Int32)(data+4); // TopRef 459
                reni_1((   data+4))                  ; // Call   515
                                                               
                
            };
        }
        
        // Reni.Syntax.Struct
        // {
        //     _data=Reni.Struct.Container
        //     {
        //         C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.4(2,0): see there: 
        //         
        //     }
        //     ObjectId=149
        // }
        private static void reni_0(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[2])
            {
                (*(sbyte*) ( data+1)) = (sbyte)(127)       ; // BitArray                       671
                (*(sbyte*) (data+0)) = (*(sbyte*) (frame-1)); // TopFrame                       877
                (*(sbyte*) (data+1)) = (*(sbyte*) (data+0)); // StatementEnd                     782
                (*(sbyte*) (frame-1)) = (*(sbyte*) (data+1)); // StorageDescriptor.FunctionReturn
                
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
        //                 ObjectId=86
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
        //                 ObjectId=85
        //             }
        //             Args=null
        //             ObjectId=6
        //         }
        //     }
        //     ObjectId=87
        // }
        private static void reni_1(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[4])
            {
                (*(Int32*) (            data+0)) = (*(Int32*) (frame-4))           ; // TopFrame    990
                (*(Int32*) (             data+0)) += -1                             ; // RefPlus     924
                (*(sbyte*) (              data+3)) = (*(sbyte*) (*(Int32*) (data+0))); // Dereference 935
                Data.DumpPrint((*(sbyte*) (data+3)))                                ; // DumpPrint   947
                ;                                //                                                 StorageDescriptor.FunctionReturn
                
            };
        }
    }
}
