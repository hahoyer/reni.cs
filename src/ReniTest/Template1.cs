#region Copyright (C) 2012

// 
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
//f: arg/\;g: f(arg)/\;x:4; g(x)dump_print
//
        public static void MainFunction()
        {
            var data = Data.Create(5);
            data.Push(4);
            data.Push(data.Address(0));
            var h_3_0 = data.Pull(4);
            data.Push(h_3_0.Get(4, 0));
            var frame = data.Pull(4);
            var result = Function0(frame);
            data.Push(result);
            data.Push(data.Pull(1).BitCast(4).BitCast(8));
            var h_3_1 = data.Pull(1);
            data.Push(h_3_1.Get(1, 0));
            data.Pull(1).PrintNumber();
        }
        // f.29(arg)
        static Data Function0(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.GetFromBack(4, -4));
            var frame1 = data.Pull(4);
            var function1 = Function1(frame1);
            data.Push(function1);
            return data;
        }
        // arg
        static Data Function1(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.GetFromBack(4, -4));
            data.Push(data.Pull(4).Dereference(1).BitCast(4).BitCast(8));
            return data;
        }
    }
}