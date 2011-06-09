using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class IntegerSyntax : ParsedSyntax, IComparableEx<IntegerSyntax>
    {
        public IntegerSyntax(TokenData token)
            : base(token) { }

        [DisableDump]
        internal override Set<string> Variables { get { return new Set<string>(); } }

        internal override string SmartDump(ISmartDumpToken @operator) { return Token.Name; }
        public int CompareToEx(IntegerSyntax other) { return 0; }
    }
}