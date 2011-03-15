using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class NumberSyntax : ParsedSyntax, IComparableEx<NumberSyntax>
    {
        internal BigRational Value;

        internal NumberSyntax(TokenData token)
            : base(token) { Value = BigInteger.Parse(token.Name); }

        internal NumberSyntax(BigRational value)
            : base(null) { Value = value; }

        [IsDumpEnabled(false)]
        internal override Set<string> Variables { get { return new Set<string>(); } }

        internal override bool IsDistinct(ParsedSyntax other) { throw new NotImplementedException(); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Value.ToString(); }
        internal override ParsedSyntax Times(BigRational value) { return new NumberSyntax(Value*value); }
        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions) { return DefaultReplace(); }
        public int CompareToEx(NumberSyntax other) { return Value.CompareTo(other.Value); }
    }
}