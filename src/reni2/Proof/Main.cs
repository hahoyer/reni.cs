using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    public static class Main
    {
        public static void Run()
        {
            var statement = new Holder(@"
a elem Integer & 
b elem Integer & 
c elem Integer & 
a^2 + b^2 = c^2 &
a gcd b = 1 &
c + a = x &
c - a = y
");
            var xx = statement.Variables;
            Tracer.FlaggedLine("Variables: " + xx.Format(", "));
        }
    }


    internal class TypeOperator : TokenClass
    {}

    internal class KGV : TokenClass
    {}

    internal class Star : TokenClass
    {}

    internal class Slash : TokenClass
    {}

    internal class Exclamation : TokenClass
    {}

    internal class NotEqual : TokenClass
    {}

    internal class CompareOperator : TokenClass
    {}
}