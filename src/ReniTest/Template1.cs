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
        // f: arg/\(arg+value)dump_print;f(100) := 2; 
        public static void MainFunction()
        {
            var data = Data.Create(8);
            data.Push(100);
            var v100 = data.Pull(1);
            data.Push(v100.Address(0));
            var a100 = data.Pull(4);
            data.Push(2);
            var v2 = data.Pull(1);
            data.Push(v2.Get(1, 0).BitCast(3).BitCast(8));
            var v2x = data.Pull(1);
            data.Push(a100.Address(0));
            data.Push(v2x.Address(0));
            data.Push(SetFunction0(data.Pull(8)));
        }

        // arg 
        static Data GetFunction0(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).Dereference(1));

            return data;
        }

        // ((arg)+.13(value))dump_print.22 
        static Data SetFunction0(Data frame)
        {
            var data = Data.Create(5);
            data.Push(frame.Get(4, 4));
            data.Push(data.Pull(4).Dereference(1));
            var h_4_0 = data.Pull(1);
            data.Push(h_4_0.Get(1, 0));
            data.Push(frame.Get(4, 0));
            data.Push(data.Pull(4).Dereference(1));
            data.Plus(sizeBytes: 2, leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(2).BitCast(9).BitCast(16));
            var h_4_1 = data.Pull(2);
            data.Push(h_4_1.Get(2, 0));
            data.Pull(2).PrintNumber();

            return data;
        }
    }
}