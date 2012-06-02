#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using System.Numerics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Runtime
{
    sealed class BiasCache
    {
        readonly List<BigInteger> _data = new List<BigInteger>();
        readonly BigInteger _maxDistance;
        public BiasCache(BigInteger maxDistance) { _maxDistance = maxDistance; }

        public string AddressDump(Data data)
        {
            var number = new BigInteger(data.GetBytes());
            var splitted = Split(number);
            return "b" + splitted.Item1 + (splitted.Item2==0?"":"+" + splitted.Item2);
        }

        Tuple<int, BigInteger> Split(BigInteger value)
        {
            if(_data.Count > 0)
            {
                var minDistanceIndex = _data.Select(d => Distance(d, value)).MinIndexList().First();
                if(Distance(_data[minDistanceIndex], value) <= _maxDistance)
                    return new Tuple<int, BigInteger>(minDistanceIndex, value - _data[minDistanceIndex]);
            }

            return AddBase(value);
        }

        Tuple<int, BigInteger> AddBase(BigInteger value)
        {
            _data.Add(value);
            return new Tuple<int, BigInteger>(_data.Count - 1, 0);
        }

        static BigInteger Distance(BigInteger a, BigInteger b)
        {
            if (a < b)
                return b - a;
            return a - b;
        }
    }
}