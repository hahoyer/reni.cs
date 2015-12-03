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
            var a = "abc";
            var r = new DominatorRelation
                (
                a,
                "-+-",
                "--+",
                "+--"
                );

            var allWords = 10.Select(x => x).SelectMany(i => AllWords(a, i)).ToArray();
            var dd = allWords.Select
                (
                    c => new
                    {
                        x = c,
                        d = r.Dominators(c)
                    }).ToArray();
            var d0 = dd.Where(x => x.d.Length == 0).ToArray();
            var d1 = dd.Where(x => x.d.Length == 1).ToArray();
            var d2 = dd.Where(x => x.d.Length > 1).ToArray();
            var d21 = d2.FirstOrDefault();
        }

        static IEnumerable<string> AllWords(string a, int length)
        {
            if(length == 0)
                return new[] {""};
            return AllWords(a, length - 1)
                .SelectMany(x => a.Select(c => x + c));
        }
    }

    static class Word
    {
        internal static string LeftWord(this string data, int item) => data.Substring(0, item);
        internal static string RightWord(this string data, int item) => data.Substring(item + 1);
    }



}