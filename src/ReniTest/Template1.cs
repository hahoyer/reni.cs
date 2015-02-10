
// 2015-02-09 23:33:50.460 

using System;
using Reni;
using Reni.Runtime;

namespace ReniTest
{
	unsafe static public class Reni
	{	    
        // 
//d: <<1;
//ref: d array_reference;
//(ref >> 0) dump_print;
// 
		unsafe static public void MainFunction()
		{
	    	
            var data = Data.Create(13);
            data.Push(1);
            var h_0_0 = data.Pull(1);
            data.Push(h_0_0.Get(1, 0));
            data.Push(data.Pointer(0));
            data.Push(0);
            var h_2_0 = data.Pull(1);
            data.Push(data.Pointer(0));
            data.Push(h_2_0.Get(1, 0).BitCast(1).BitCast(32));
            data.Plus(sizeBytes:4, leftBytes:4, rightBytes:4);
            data.Push(data.Pull(4).DePointer(1).BitCast(2).BitCast(8));
            data.Pull(1).PrintNumber();
            
        }
	}
}

     