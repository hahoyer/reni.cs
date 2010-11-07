using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Runtime;

namespace Reni
{
    /// <summary>
    /// sss
    /// </summary>
    public static unsafe class ReniTest
    {
        public static void MainFunction()
        {
            var list2 = new DataContainer();
            var list3 = new DataContainer();
            list3.Expand((new DataContainer(1)).Minus(1).BitCast(1, 2));
            list3.Expand((new DataContainer(12)).Minus(1).BitCast(1, 5));
            list3.Expand((new DataContainer(123)).Minus(1));
            list3.Expand((new DataContainer(210, 4)).Minus(2).BitCast(2, 12));
            list3.Expand((new DataContainer(57, 48)).Minus(2).BitCast(2, 15));
            list3.Expand((new DataContainer(64, 226, 1)).Minus(3).BitCast(3, 18));
            list3.Expand((new DataContainer(135, 214, 18)).Minus(3).BitCast(3, 22));
            list3.Expand((new DataContainer(78, 97, 188, 0)).Minus(4).BitCast(4, 25));
            list3.Expand((new DataContainer(21, 205, 91, 7)).Minus(4).BitCast(4, 28));
            list3.Expand((new DataContainer(210, 2, 150, 73)).Minus(4));
            var local10Index0 = list3;
            list2.Drop();
            ;
        }
    }
}