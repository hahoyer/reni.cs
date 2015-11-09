using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;


namespace Theory
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new DominatorRelation
            {
                Data = new[]
                {
                    new Tuple<char, char>('+', 'a'),
                    new Tuple<char, char>('+', '*'),
                    new Tuple<char, char>('*', 'a')
                }
            };


            var x = "a*a+a*a";
            var d = r.Dominators(x).ToArray();
        }
    }

    static class Word
    {
        internal static string LeftWord(this string data, int item) { return data.Substring(0, item); }
        internal static string RightWord(this string data, int item) { return data.Substring(item + 1); }
    }

    internal class DominatorRelation
    {
        internal Tuple<char, char>[] Data;

        internal bool IsDominator(char left, char right)
        {
            return Data.Any(item => item.Item1 == left && item.Item2 == right);
        }

        internal bool IsDominator(int position, string data)
        {
            if(Dominators(data.LeftWord(position)).Any(item => IsDominator(data[item], data[position])))
                return false;

            return Dominators(data.RightWord(position))
                .All(item => IsDominator(data[position], data[position + 1 + item]));
        }

        internal IEnumerable<int> Dominators(string data)
            => data.Length.Select().Where(position => IsDominator(position, data));
    }
}