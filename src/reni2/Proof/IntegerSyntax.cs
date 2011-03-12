using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class IntegerSyntax : ParsedSyntax
    {
        public IntegerSyntax(TokenData token)
            : base(token) { }

        internal override Set<string> Variables { get { return new Set<string>(); } }
    }
}