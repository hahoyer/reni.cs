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
        // i: 10; f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f() 
        public static void MainFunction()
        {
            var data = Data.Create(5);
            data.Push(10);
            data.Push(data.Address(1));
            data.Push(GetFunction0(data.Pull(4)));
        }

        // ((i)>(0))then(container.0) 
        static Data GetFunction0(Data frame)
        {
            Start:

            var data = Data.Create(8);
            data.Push(0);
            var h_3_0 = data.Pull(1);
            data.Push(frame.Get(4, 0));
            data.RefPlus(-1);
            data.Push(data.Pull(4).Dereference(1).BitCast(5).BitCast(8));
            data.Push(h_3_0.Get(1, 0).BitCast(1).BitCast(8));
            data.Greater(sizeBytes: 1, leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(1).BitCast(1).BitCast(8));
            if(data.Pull(1).GetBytes()[0] != 0)
            {
                ;
                data.Push(1);
                var h_4_0 = data.Pull(1);
                data.Push(frame.Get(4, 0));
                data.RefPlus(-1);
                data.Push(data.Pull(4).Dereference(1).BitCast(5).BitCast(8));
                data.Push(h_4_0.Get(1, 0).BitCast(2).BitCast(8));
                data.Minus(sizeBytes: 1, leftBytes: 1, rightBytes: 1);
                data.Push(data.Pull(1).BitCast(6).BitCast(8));
                var h_4_1 = data.Pull(1);
                data.Push(h_4_1.Get(1, 0).BitCast(6).BitCast(8));
                var h_4_2 = data.Pull(1);
                data.Push(h_4_2.Get(1, 0));
                var h_4_3 = data.Pull(1);
                data.Push(h_4_3.Get(1, 0).BitCast(5).BitCast(8));
                var h_4_4 = data.Pull(1);
                data.Push(h_4_4.Get(1, 0).BitCast(5).BitCast(8));
                var h_4_5 = data.Pull(1);
                data.Push(frame.Get(4, 0));
                data.RefPlus(-1);
                data.Push(h_4_5.Address(0));
                data.Assign(1);
                data.Push(frame.Get(4, 0));
                data.RefPlus(-1);
                data.Push(data.Pull(4).Dereference(1));
                data.Pull(1).PrintNumber();
                goto Start;
            }
            else
            {
                ;
                data.Push();
            }
            ;

            return data;
        }
    }
}