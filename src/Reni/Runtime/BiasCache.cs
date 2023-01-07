using System.Numerics;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Runtime;

sealed class BiasCache
{
    readonly List<byte[]> Data = new();
    readonly BigInteger MaxDistance;
    public BiasCache(BigInteger maxDistance) => MaxDistance = maxDistance;

    string Dump(Data data, bool isAddress)
    {
        var splitted = Split(data.GetBytes(), isAddress);
        return splitted == null? data.GetBytes(1).Single().ToString() : Dump(splitted.Value);
    }

    static string Dump((int, BigInteger) value)
    {
        var offset = "";
        if(value.Item2 > 0)
            offset = "+";
        if(value.Item2 != 0)
            offset += value.Item2;
        return "b" + value.Item1 + offset;
    }

    public string AddressDump(Data data) => Dump(data, true);

    (int, BigInteger) AddBase(byte[] data)
    {
        (data.Length == DataHandler.RefBytes).Assert();
        Data.Add(data);
        return (Data.Count - 1, 0);
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
        return result == null? null : Dump(result.Value);
    }

    (int, BigInteger)? Split(byte[] data, bool isAddress)
    {
        (data.Length == DataHandler.RefBytes).Assert();
        var value = new BigInteger(data);

        if(Data.Count > 0)
        {
            var minDistanceIndex = Data.Select(d => Distance(d, value)).MinIndexList().First();
            if(Distance(Data[minDistanceIndex], value) <= MaxDistance)
                return
                    (minDistanceIndex, value - new BigInteger(Data[minDistanceIndex]));
        }

        if(isAddress)
            return AddBase(data);

        return null;
    }
}