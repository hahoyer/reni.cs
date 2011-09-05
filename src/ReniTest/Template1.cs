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
//x: 100; f: x/\;f() dump_print;
//
        public static void MainFunction()
        {
            var data = Data.Create(5);
            data.Push(100);
            data.Push(data.Address(1));
            data.Push(Function0(data.Pull(4)));
            var h_2_0 = data.Pull(1);
            data.Push(h_2_0.Get(1, 0));
            data.Pull(1).PrintNumber();
        }
        // x.27
        static Data Function0(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.GetFromBack(4, -4));
            data.RefPlus(-1);
            data.Push(data.Pull(4).Dereference(1));
            return data;
        }
    }
}                                                            