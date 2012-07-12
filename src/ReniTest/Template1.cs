#region Copyright (C) 2012

//     Project reniTest
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Runtime;

namespace ReniTest
{
    unsafe static public class Reni
    {
        // ((<<5<<3<<5<<1<<3) (3)) dump_print 
        unsafe static public void MainFunction()
        {

            var data = Data.Create(12);
            data.Push(3);
            var h_0_0 = data.Pull(1);
            data.Push(h_0_0.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_1 = data.Pull(1);
            data.Push(h_0_1.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_2 = data.Pull(1);
            data.Push(1);
            var h_0_3 = data.Pull(1);
            data.Push(h_0_3.Get(1, 0).BitCast(2).BitCast(8));
            var h_0_4 = data.Pull(1);
            data.Push(h_0_4.Get(1, 0).BitCast(2).BitCast(8));
            var h_0_5 = data.Pull(1);
            data.Push(5);
            var h_0_6 = data.Pull(1);
            data.Push(3);
            var h_0_7 = data.Pull(1);
            data.Push(h_0_7.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_8 = data.Pull(1);
            data.Push(h_0_8.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_9 = data.Pull(1);
            data.Push(5);
            var h_0_10 = data.Pull(1);
            data.Push(h_0_9.Get(1, 0).BitCast(4).BitCast(8));
            data.Push(h_0_10.Get(1, 0));
            var h_0_11 = data.Pull(2);
            data.Push(h_0_6.Get(1, 0).BitCast(4).BitCast(8));
            data.Push(h_0_11.Get(2, 0));
            var h_0_12 = data.Pull(3);
            data.Push(h_0_5.Get(1, 0).BitCast(4).BitCast(8));
            data.Push(h_0_12.Get(3, 0));
            var h_0_13 = data.Pull(4);
            data.Push(h_0_2.Get(1, 0).BitCast(4).BitCast(8));
            data.Push(h_0_13.Get(4, 0));
            var h_0_14 = data.Pull(5);
            data.Push(3);
            var h_0_15 = data.Pull(1);
            data.Push(h_0_15.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_16 = data.Pull(1);
            data.Push(h_0_16.Get(1, 0).BitCast(3).BitCast(8));
            var h_0_17 = data.Pull(1);
            data.Push(h_0_14.Address(0));
            data.Push(h_0_17.Get(1, 0));
            data.Push(data.Pull(1).BitCast(32));
            data.Push(1, 0, 0, 0);
            data.Star(4, 4, 4);
            data.Plus(4, 4, 4);
            data.Push(data.Pull(4).Dereference(1));
            data.Pull(1).PrintNumber();

        }
    }
}