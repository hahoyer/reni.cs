
// 2015-02-08 22:16:42.001 

using System;
using Reni;
using Reni.Runtime;

namespace ReniTest
{
	unsafe static public class Reni
	{	    
        // 
//d: 'abcd';
//ref: d array_reference;
//(ref >> 0) dump_print;
//(ref >> 1) dump_print;
//(ref >> 2) dump_print;
//(ref >> 3) dump_print
// 
		unsafe static public void MainFunction()
		{
	    	
            var data = Data.Create(16);
            data.Push(97, 98, 99, 100);
            data.Push(data.Pointer(0));
            data.Push(0);
            var h_2_0 = data.Pull(1);
            data.Push(data.Pointer(0));
            data.Push(h_2_0.Get(1, 0).BitCast(1).BitCast(32));
            data.Plus(sizeBytes:4, leftBytes:4, rightBytes:4);
            data.Push(data.Pull(4).DePointer(1));
            data.Pull(1).PrintText(1);
            data.Push(1);
            var h_3_0 = data.Pull(1);
            data.Push(data.Pointer(0));
            data.Push(h_3_0.Get(1, 0).BitCast(2).BitCast(32));
            data.Plus(sizeBytes:4, leftBytes:4, rightBytes:4);
            data.Push(data.Pull(4).DePointer(1));
            data.Pull(1).PrintText(1);
            data.Push(2);
            var h_4_0 = data.Pull(1);
            data.Push(data.Pointer(0));
            data.Push(h_4_0.Get(1, 0).BitCast(3).BitCast(32));
            data.Plus(sizeBytes:4, leftBytes:4, rightBytes:4);
            data.Push(data.Pull(4).DePointer(1));
            data.Pull(1).PrintText(1);
            data.Push(3);
            var h_5_0 = data.Pull(1);
            data.Push(data.Pointer(0));
            data.Push(h_5_0.Get(1, 0).BitCast(3).BitCast(32));
            data.Plus(sizeBytes:4, leftBytes:4, rightBytes:4);
            data.Push(data.Pull(4).DePointer(1));
            data.Pull(1).PrintText(1);
            
        }
	}
}

     