﻿
// 2011-09-08 20:49:07.922      

using System;
using Reni;
using Reni.Runtime;

namespace ReniTest
{
	unsafe static public class Reni
	{
			// 
//f: {1000 type({arg = 1 then 1 else (arg * f[arg type((arg-1)enable_cut)])}enable_cut)}/\;f(4)dump_print
//
					unsafe static public void MainFunction( )
			{
					
var data = Data.Create(4);
data.Push(4);
var h_1_0 = data.Pull(1);
data.Push(h_1_0.Address(0));
data.Push(Function0(data.Pull(4)));
var h_1_1 = data.Pull(2);
data.Push(h_1_1.Get(2, 0));
data.Pull(2).PrintNumber();
			}
			// (1000)type((((arg)=.4(1))then(1)else((arg)*.16(f.27((arg)type(((arg)-.11(1))enable_cut.21)))))enable_cut.21)
					unsafe static Data Function0(Data frame)
			{
			Start:
					
var data = Data.Create(4);
data.Push(frame.GetFromBack(4, -4));
var h_3_0 = data.Pull(4);
data.Push(1);
var h_3_1 = data.Pull(1);
data.Push(h_3_0.Get(4, 0));
data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
data.Push(h_3_1.Get(1, 0).BitCast(2).BitCast(8));
data.Equal(leftBytes:1, rightBytes:1);
data.Push(data.Pull(1).BitCast(1).BitCast(8));
if(data.Pull(1).GetBytes()[0] != 0)
{;
    data.Push(1, 0);
}
else
{;
    data.Push(frame.GetFromBack(4, -4));
    var h_5_0 = data.Pull(4);
    data.Push(frame.GetFromBack(4, -4));
    var h_5_1 = data.Pull(4);
    data.Push(1);
    var h_5_2 = data.Pull(1);
    data.Push(h_5_1.Get(4, 0));
    data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
    data.Push(h_5_2.Get(1, 0).BitCast(2).BitCast(8));
    data.Minus(leftBytes:1, rightBytes:1);
    data.Push(data.Pull(1).BitCast(5).BitCast(8));
    var h_5_3 = data.Pull(1);
    data.Push(h_5_3.Get(1, 0).BitCast(5).BitCast(8));
    var h_5_4 = data.Pull(1);
    data.Push(h_5_4.Get(1, 0).BitCast(4).BitCast(8));
    var h_5_5 = data.Pull(1);
    data.Push(h_5_5.Address(0));
    var h_5_6 = data.Pull(4);
    data.Push(h_5_6.Get(4, 0));
    data.Push(Function0(data.Pull(4)));
    var h_5_7 = data.Pull(2);
    data.Push(h_5_0.Get(4, 0));
    data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
    data.Push(h_5_7.Get(2, 0).BitCast(11).BitCast(16));
    data.Star(sizeBytes:2, leftBytes:1, rightBytes:2);
    data.Push(data.Pull(2).BitCast(14).BitCast(16));
};
var h_6_0 = data.Pull(2);
data.Push(h_6_0.Get(2, 0).BitCast(14).BitCast(16));
var h_6_1 = data.Pull(2);
data.Push(h_6_1.Get(2, 0).BitCast(11).BitCast(16));
return data;			}
		}
}

     
