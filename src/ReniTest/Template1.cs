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
        // 
        //x: 'Hallo World';
        //y: (x(0) type * 1000) reference (x);
        //y(0) dump_print;
        //y(1) dump_print;
        //y(2) dump_print;
        //y(3) dump_print;
        //y(4) dump_print;
        //y(5) dump_print;
        //y(6) dump_print;
        // 
        public static void MainFunction()
        {
            var data = Data.Create(23);
            data.Push(72, 97, 108, 108, 111, 32, 87, 111, 114, 108, 100);
            data.Push(0);
            var h_1_0 = data.Pull(1);
            data.Push(h_1_0.Get(1, 0).BitCast(1).BitCast(8));
            var h_1_1 = data.Pull(1);
            data.Push(h_1_1.Get(1, 0).BitCast(1).BitCast(8));
            var h_1_2 = data.Pull(1);
            data.Push(data.Address(0));
            data.Push(h_1_2.Get(1, 0).BitCast(8).BitCast(32));
            data.Plus(sizeBytes: 4, leftBytes: 4, rightBytes: 4);
            var h_2_0 = data.Pull(1);
            data.Push(h_2_0.Get(1, 0).BitCast(1).BitCast(8));
            var h_2_1 = data.Pull(1);
            data.Push(h_2_1.Get(1, 0).BitCast(1).BitCast(8));
            var h_2_2 = data.Pull(4).Dereference(1);
            data.Push(data.Address(0));
            data.Push(h_2_2.Get(1, 0).BitCast(8).BitCast(32));
            data.Plus(sizeBytes: 4, leftBytes: 4, rightBytes: 4);
            data.Push(data.Pull(4).Dereference(1));
            data.Pull(1).PrintText(1);
        }
    }
}