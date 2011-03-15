using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Runtime;

namespace Reni
{
    /// <summary>
    ///     sss
    /// </summary>
    public static class ReniTest
    {
        public static void MainFunction()
        {
            var data = new byte[7];
            //List 7
            /* Reni.Code.BitArray.180 Size=16 Data="00DE"[9] */
            Data.Set(data, 5, 222, 0);
            /* Reni.Code.BitArray.244 Size=8 Data="0100"[4] */
            Data.Set(data, 4, 4);
            /* Reni.Code.LocalVariables.5869 Size=0 Holder=local3Index{0} CodeBases=
            {
                [0] Reni.Code.TopRef.5862 Size=32 Offset=0 Reason=Context.ConstructorResult{}
                [1] Reni.Code.Fiber.15 Size=24
                {
                    [*] Reni.Code.LocalVariableReference.4260 Size=32 Holder=local3Index0 Offset=0{}
                    [0] Reni.Code.Dereference.4145 DataSize=24{}
                }
                [2] Reni.Code.LocalVariableReference.4262 Size=32 Holder=local3Index1 Offset=0{}
                    
            } */
            //LocalVariables 5869
            /* Reni.Code.TopRef.5862 Size=32 Offset=0 Reason=Context.ConstructorResult */
            Data.SetFromPointer(data, 0, 4, data, 4);
            var local3Index0 = Data.Get(data, 0, 4);
            /* Reni.Code.Fiber.15 Size=24 */
            //Fiber 15
            /* Reni.Code.LocalVariableReference.4260 Size=32 Holder=local3Index0 Offset=0 */
            Data.SetFromPointer(data, 0, 4, local3Index0, 0);
            /* Reni.Code.Dereference.4145 DataSize=24 */
            Data.Dereference(data, 0, 4, 4);
            Data.Dereference(data, 0, 4, 3);

            var local3Index1 = Data.Get(data, 1, 3);
            /* Reni.Code.LocalVariableReference.4262 Size=32 Holder=local3Index1 Offset=0 */
            Data.SetFromPointer(data, 0, 4, local3Index1, 0);
            var local3Index2 = Data.Get(data, 0, 4);

            /* Reni.Code.Fiber.18 Size=0 */
            //Fiber 18
            /* Reni.Code.LocalVariableReference.4264 Size=32 Holder=local3Index2 Offset=0 */
            Data.SetFromPointer(data, 0, 4, local3Index2, 0);
            /* Reni.Code.Dereference.4230 DataSize=4 */
            Data.Dereference(data, 0, 4, 4);
            Data.Dereference(data, 0, 4, 1);
            /* Reni.Code.BitCast.3 TargetSize=8 SignificantSize=4 */
            Data.BitCast(data, 3, 1, 4);
            /* Reni.Code.DumpPrintOperation.2973 <8> dump_print <0> */
            Data.DumpPrint(data, 3, 1);
        }
    }
}