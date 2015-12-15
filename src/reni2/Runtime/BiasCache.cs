using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using hw.Helper;

namespace Reni.Runtime
{
    sealed class BiasCache
    {
        readonly List<BigInteger> _data = new List<BigInteger>();
        readonly BigInteger _maxDistance;
        public BiasCache(BigInteger maxDistance) { _maxDistance = maxDistance; }

        string Dump(Data data, bool isAddress)
        {
            var number = new BigInteger(data.GetBytes());
            var splitted = Split(number, isAddress);
            if(splitted == null)
                return data.GetBytes(1).Single().ToString();
            var offset = "";
            if(splitted.Item2 > 0)
                offset = "+";
            if(splitted.Item2 != 0)
                offset += splitted.Item2;
            return "b" + splitted.Item1 + offset;
        }

        public string Dump(Data data) => Dump(data, false);
        public string AddressDump(Data data) => Dump(data, true);

        Tuple<int, BigInteger> Split(BigInteger value, bool isAddress)
        {
            if(_data.Count > 0)
            {
                var minDistanceIndex = _data.Select(d => Distance(d, value)).MinIndexList().First();
                if(Distance(_data[minDistanceIndex], value) <= _maxDistance)
                    return new Tuple<int, BigInteger>
                        (minDistanceIndex, value - _data[minDistanceIndex]);
            }

            return isAddress ? AddBase(value) : null;
        }

        Tuple<int, BigInteger> AddBase(BigInteger value)
        {
            _data.Add(value);
            return new Tuple<int, BigInteger>(_data.Count - 1, 0);
        }

        static BigInteger Distance(BigInteger a, BigInteger b)
        {
            if(a < b)
                return b - a;
            return a - b;
        }
    }
}