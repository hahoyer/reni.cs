using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Runtime
{
    sealed class BiasCache
    {
        readonly List<byte[]> _data = new List<byte[]>();
        readonly BigInteger _maxDistance;
        public BiasCache(BigInteger maxDistance) { _maxDistance = maxDistance; }

        string Dump(Data data, bool isAddress)
        {
            var splitted = Split(data.GetBytes(), isAddress);
            return splitted == null ? data.GetBytes(1).Single().ToString() : Dump(splitted);
        }

        static string Dump(Tuple<int, BigInteger> value)
        {
            var offset = "";
            if(value.Item2 > 0)
                offset = "+";
            if(value.Item2 != 0)
                offset += value.Item2;
            return "b" + value.Item1 + offset;
        }

        public string AddressDump(Data data) => Dump(data, true);

        Tuple<int, BigInteger> AddBase(byte[] data)
        {
            Tracer.Assert(data.Length == DataHandler.RefBytes);
            _data.Add(data);
            return new Tuple<int, BigInteger>(_data.Count - 1, 0);
        }

        static BigInteger Distance(byte[] aa, BigInteger b)
        {
            var a = new BigInteger(aa);

            if(a < b)
                return b - a;
            return a - b;
        }

        public string Dump(byte[] data, int position)
        {
            if(position + DataHandler.RefBytes > data.Length)
                return null;

            var result = Split(data.Get(position, DataHandler.RefBytes), false);
            return result != null ? Dump(result) : null;
        }

        Tuple<int, BigInteger> Split(byte[] data, bool isAddress)
        {
            Tracer.Assert(data.Length == DataHandler.RefBytes);
            var value = new BigInteger(data);

            if(_data.Count > 0)
            {
                var minDistanceIndex = _data.Select(d => Distance(d, value)).MinIndexList().First();
                if(Distance(_data[minDistanceIndex], value) <= _maxDistance)
                    return new Tuple<int, BigInteger>
                        (minDistanceIndex, value - new BigInteger(_data[minDistanceIndex]));
            }

            return isAddress ? AddBase(data) : null;
        }
    }
}