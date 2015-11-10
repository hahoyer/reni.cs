using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Theory
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new DominatorRelation
                (
                "     ==",
                "a*+()()",
                "---+-++",
                "+--+-++",
                "++-+-++",
                "++++-??",
                "----???",
                "---????",
                "---????"
                );
            var x = "a*(a+a)*a";
            var dd = r.Dominators(x);
            var d = dd.FirstOrDefault();
        }
    }

    static class Word
    {
        internal static string LeftWord(this string data, int item) => data.Substring(0, item);
        internal static string RightWord(this string data, int item) => data.Substring(item + 1);
    }
}