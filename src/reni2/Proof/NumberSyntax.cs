using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class NumberSyntax : ParsedSyntax
    {
        internal BigInteger Value;

        internal NumberSyntax(TokenData token)
            : base(token) { Value = BigInteger.Parse(token.Name); }

        internal override Set<string> Variables { get { return new Set<string>(); } }
    }
}