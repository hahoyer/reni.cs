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
    public static class Reni
    {
        // x: 100;f: arg+x/\;f(2) dump_print; 
        public static void MainFunction()
        {
            var data = Data.Create(9);
            data.Push(100);
            data.Push(2);
            var h_1_0 = data.Pull(1);
            data.Push(data.Address(1));
            data.Push(h_1_0.Address(0));
            var h_1_1 = data.Pull(8);
            data.Push(h_1_1.Get(8, 0));
            data.Push(GetFunction0(data.Pull(8)));
            data.Push(data.Pull(2).BitCast(9).BitCast(16));
            var h_1_2 = data.Pull(2);
            data.Push(h_1_2.Get(2, 0));
            data.Pull(2).PrintNumber();
        }

        // (arg)+.13(x.29) 
        static Data GetFunction0(Data frame)
        {
            var data = Data.Create(5);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).Dereference(1).BitCast(3).BitCast(8));
            var h_4_0 = data.Pull(1);
            data.Push(h_4_0.Get(1, 0).BitCast(3).BitCast(8));
            data.Push(frame.Get(4, 4));
            data.RefPlus(-1);
            data.Push(data.Pull(4).Dereference(1));
            data.Plus(sizeBytes: 2, leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(2).BitCast(9).BitCast(16));

            return data;
        }
    }
}