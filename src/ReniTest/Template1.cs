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
//f: arg/\;
//repeat: /\arg while() then(arg body(), repeat(arg));
//
//count: 10,
//index: count type instance(0),
//repeat
//(
//    while: /\ index < count, 
//    body: /\
//    (
//        index dump_print, 
//        ' ' dump_print, 
//        index := (index + 1)enable_cut
//    )
//)
// 
        public static void MainFunction()
        {
            var data = Data.Create(6);
            data.Push(10);
            data.Push(0);
            var h_1_0 = data.Pull(1);
            data.Push(h_1_0.Get(1, 0).BitCast(1).BitCast(8));
            var h_1_1 = data.Pull(1);
            data.Push(h_1_1.Get(1, 0).BitCast(1).BitCast(8));
            var h_1_2 = data.Pull(1);
            data.Push(h_1_2.Get(1, 0).BitCast(5).BitCast(8));
            data.Push(data.Address(2));
            data.Push(GetFunction0(data.Pull(4)));
        }

        // ((arg.38)while(().46).47)then(container.0.0).82 
        static Data GetFunction0(Data frame)
        {
            Start:

            var data = Data.Create(4);
            data.Push(frame.Get(4, 0));
            data.Push(GetFunction1(data.Pull(4)));
            var h_4_0 = data.Pull(1);
            data.Push(h_4_0.Get(1, 0));
            if(data.Pull(1).GetBytes()[0] != 0)
            {
                ;
                data.Push(frame.Get(4, 0));
                data.Push(GetFunction2(data.Pull(4)));
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

        // (index.144)<(count.145).146 
        static Data GetFunction1(Data frame)
        {
            var data = Data.Create(4);
            data.Push(frame.Get(4, 0));
            data.ReferencePlus(-1);
            data.Push(data.Pull(4).Dereference(1).BitCast(5).BitCast(8));
            var h_10_0 = data.Pull(1);
            data.Push(frame.Get(4, 0));
            data.ReferencePlus(-2);
            data.Push(data.Pull(4).Dereference(1).BitCast(5).BitCast(8));
            data.Push(h_10_0.Get(1, 0).BitCast(5).BitCast(8));
            data.Less(sizeBytes: 1, leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(1).BitCast(1).BitCast(8));

            return data;
        }

        // container.1.1 
        static Data GetFunction2(Data frame)
        {
            var data = Data.Create(8);
            data.Push(frame.Get(4, 0));
            data.ReferencePlus(-2);
            data.Push(data.Pull(4).Dereference(1));
            data.Pull(1).PrintNumber();
            data.Push(32);
            var h_12_0 = data.Pull(1);
            data.Push(h_12_0.Get(1, 0));
            data.Pull(1).PrintText(1);
            data.Push(1);
            var h_13_0 = data.Pull(1);
            data.Push(frame.Get(4, 0));
            data.ReferencePlus(-2);
            data.Push(data.Pull(4).Dereference(1).BitCast(5).BitCast(8));
            data.Push(h_13_0.Get(1, 0).BitCast(2).BitCast(8));
            data.Plus(sizeBytes: 1, leftBytes: 1, rightBytes: 1);
            data.Push(data.Pull(1).BitCast(6).BitCast(8));
            var h_13_1 = data.Pull(1);
            data.Push(h_13_1.Get(1, 0).BitCast(6).BitCast(8));
            var h_13_2 = data.Pull(1);
            data.Push(h_13_2.Get(1, 0).BitCast(6).BitCast(8));
            var h_13_3 = data.Pull(1);
            data.Push(h_13_3.Get(1, 0).BitCast(5).BitCast(8));
            var h_13_4 = data.Pull(1);
            data.Push(frame.Get(4, 0));
            data.ReferencePlus(-2);
            data.Push(h_13_4.Address(0));
            data.Assign(1);

            return data;
        }
    }
}