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
        public static void reni() {
            fixed(sbyte*data=new sbyte[6])
            {
                (*(sbyte*) (data+5)) = (sbyte)(0) ; // BitArray 276
                reni_0((   data+6))                ; // Call     461
                (*(sbyte*) (data+4)) = (sbyte)(23)  ; // BitArray 545
                reni_1((   data+5))                  ; // Call   909
                (*(Int32*) (data+0)) = (Int32)(data+5); // TopRef 1266
                reni_3((   data+4))                  ; // Call   1251
                                                               
                
            };
        }
        
        // Reni.Struct.Container
        // {
        //     C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.5(2,0): see there: 
        //     
        // }
        // Reni.Syntax.Struct
        // {
        //     _data=Reni.Struct.Container
        //     {
        //         C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.5(2,0): see there: 
        //         
        //     }
        //     ObjectId=175
        // }
        private static void reni_0(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[2])
            {
                StartFunction:                                                               
                (*(sbyte*) ( data+1)) = (sbyte)(127)       ; // BitArray                       1553
                (*(sbyte*) (data+0)) = (*(sbyte*) (frame-1)); // TopFrame                       1774
                (*(sbyte*) (data+1)) = (*(sbyte*) (data+0)); // StatementEnd                     1664
                (*(sbyte*) (frame-1)) = (*(sbyte*) (data+1)); // StorageDescriptor.FunctionReturn
                
            };
        }
        
        // Integer8(arg)
        // Reni.Syntax.Statement
        // {
        //     Chain=Count=1
        //     {
        //         [0] Reni.Syntax.MemberElem
        //         {
        //             DefineableToken=Reni.Parser.DefineableToken
        //             {
        //                 TokenClass=Reni.Parser.TokenClass.UserSymbol
        //                 {
        //                     PotentialTypeName=.TokenClass.Name.TInteger8T
        //                     DataFunctionName=UserSymbol
        //                     ObjectId=18
        //                 }
        //                 ObjectId=60
        //             }
        //             Args=arg
        //             Reni.Syntax.Special
        //             {
        //                 _token=Reni.Parser.Token
        //                 {
        //                     TokenClass=Reni.Parser.TokenClass.Name.TargT{ObjectId=20}
        //                     NodeDump=C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\Create.reni(6,33): arg: 
        //                     FilePosn=
        //                     C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\Create.reni(6,33): arg: 
        //                     ObjectId=20
        //                 }
        //                 ObjectId=57
        //             }
        //             ObjectId=4
        //         }
        //     }
        //     ObjectId=61
        // }
        private static void reni_1(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[4])
            {
                StartFunction:                                                                
                (*(Int32*) ( data+0)) = (Int32)(frame-1)  ; // FrameRef                         1981
                reni_2((    data+4))                       ; // Call                             1892
                (*(sbyte*) (frame-1)) = (*(sbyte*) (data+3)); // StorageDescriptor.FunctionReturn
                
            };
        }
        
        // Reni.Struct.Container
        // {
        //     C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.5(2,0): see there: 
        //     
        // }
        // Reni.Syntax.Struct
        // {
        //     _data=Reni.Struct.Container
        //     {
        //         C:\Dokumente und Einstellungen\hh\Eigene Dateien\Develop2008\Reni\out\Debug\struct.5(2,0): see there: 
        //         
        //     }
        //     ObjectId=175
        // }
        private static void reni_2(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[2])
            {
                StartFunction:                                                               
                (*(sbyte*) ( data+1)) = (sbyte)(127)       ; // BitArray                       2052
                (*(sbyte*) (data+0)) = (*(sbyte*) (frame-4)); // TopFrame                       2268
                (*(sbyte*) (data+1)) = (*(sbyte*) (data+0)); // StatementEnd                     2163
                (*(sbyte*) (frame-1)) = (*(sbyte*) (data+1)); // StorageDescriptor.FunctionReturn
                
            };
        }
        
        // _data dump_print
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
        private static void reni_3(System.SByte* frame) {
            fixed(sbyte*data=new sbyte[4])
            {
                StartFunction:                                                                    
                (*(Int32*) (            data+0)) = (*(Int32*) (frame-4))           ; // TopFrame    2383
                (*(Int32*) (             data+0)) += -1                             ; // RefPlus     2318
                (*(sbyte*) (              data+3)) = (*(sbyte*) (*(Int32*) (data+0))); // Dereference 2329
                Data.DumpPrint((*(sbyte*) (data+3)))                                ; // DumpPrint   2341
                ;                                //                                                 StorageDescriptor.FunctionReturn
                
            };
        }
    }
}
