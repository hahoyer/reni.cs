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
        // f: arg/\;f(2) dump_print; 
        public static void MainFunction()
        {
            var data = Data.Create(4);
            data.Push(2);
            var h_1_0 = data.Pull(1);
            data.Push(h_1_0.Address(0));
            var h_1_1 = data.Pull(4);
            data.Push(h_1_1.Address(4));
            data.Push(GetFunction0(data.Pull(4)));
            data.Push(data.Pull(1).BitCast(3).BitCast(8));
            var h_1_2 = data.Pull(1);
            data.Push(h_1_2.Address(0));
            data.Push(data.Pull(4).Dereference(1));
            data.Pull(1).PrintNumber();
        }

        // arg 
        static Data GetFunction0(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.Pull(4));
            data.RefPlus(-4);
            data.Push(data.Pull(4).Dereference(4).Dereference(1).BitCast(3).BitCast(8));

            return data;
        }
    }
}