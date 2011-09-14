//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
//5, (^ _A_T_ 0 + ^ _A_T_ 0)dump_print
//
        public static void MainFunction()
        {
            var data = Data.Create(5);
            data.Push(data.Address(-1));
            var h_6_0 = data.Pull(4);
            data.Push(h_6_0.Get(4, 0));
            var h_6_1 = data.Pull(4);
            data.Push(data.Address(-1));
            var h_6_2 = data.Pull(4);
            data.Push(h_6_2.Get(4, 0));
            var h_6_3 = data.Pull(4);
            data.Push(h_6_1.Get(4, 0));
            data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
            data.Push(h_6_3.Get(4, 0));
            data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
            data.Plus(leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(1).BitCast(5).BitCast(8));
            var h_6_4 = data.Pull(1);
            data.Push(5);
            data.Push(h_6_4.Get(1, 0));
            data.Pull(1).PrintNumber();
        }
    }
}